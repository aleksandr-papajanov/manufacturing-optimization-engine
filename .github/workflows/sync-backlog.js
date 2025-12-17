import fs from "fs";
import { graphql } from "@octokit/graphql";

const token = process.env.GITHUB_TOKEN;
const repoFull = process.env.GITHUB_REPOSITORY;

const PROJECT_ID = "PVT_kwHOAg280c4BKdGd";
const STATUS_FIELD_ID = "PVTSSF_lAHOAg280c4BKdGdzg6UWKY";
const STATUS_BACKLOG_ID = "b9855712";
const PARENT_FIELD_ID = "PVTF_lAHOAg280c4BKdGdzg6UWKw";

const graphqlWithAuth = graphql.defaults({
  headers: { authorization: `bearer ${token}` }
});

const backlog = JSON.parse(fs.readFileSync("backlog.json", "utf8"));

/* ---------------- helpers ---------------- */

const sleep = ms => new Promise(r => setTimeout(r, ms));

async function getRepoId() {
  const [owner, name] = repoFull.split("/");
  const res = await graphqlWithAuth(`
    query ($owner: String!, $name: String!) {
      repository(owner: $owner, name: $name) { id }
    }
  `, { owner, name });
  return res.repository.id;
}

async function findIssueByTitle(title) {
  const [owner, name] = repoFull.split("/");
  const res = await graphqlWithAuth(`
    query ($owner: String!, $name: String!, $q: String!) {
      search(type: ISSUE, query: $q, first: 1) {
        nodes {
          ... on Issue {
            id
            title
          }
        }
      }
    }
  `, {
    owner,
    name,
    q: `repo:${owner}/${name} "${title}"`
  });

  return res.search.nodes[0] ?? null;
}

async function createIssue(repoId, title, body) {
  const res = await graphqlWithAuth(`
    mutation ($repoId: ID!, $title: String!, $body: String!) {
      createIssue(input: {
        repositoryId: $repoId,
        title: $title,
        body: $body
      }) {
        issue { id title }
      }
    }
  `, { repoId, title, body });

  return res.createIssue.issue;
}

async function addToProject(issueId, parentItemId = null) {
  const res = await graphqlWithAuth(`
    mutation ($projectId: ID!, $contentId: ID!) {
      addProjectV2Item(input: {
        projectId: $projectId,
        contentId: $contentId
      }) {
        item { id }
      }
    }
  `, { projectId: PROJECT_ID, contentId: issueId });

  const itemId = res.addProjectV2Item.item.id;

  // Status = Backlog
  await graphqlWithAuth(`
    mutation {
      updateProjectV2ItemFieldValue(input: {
        projectId: "${PROJECT_ID}",
        itemId: "${itemId}",
        fieldId: "${STATUS_FIELD_ID}",
        value: { singleSelectOptionId: "${STATUS_BACKLOG_ID}" }
      }) { clientMutationId }
    }
  `);

  // Parent link
  if (parentItemId) {
    await graphqlWithAuth(`
      mutation {
        updateProjectV2ItemFieldValue(input: {
          projectId: "${PROJECT_ID}",
          itemId: "${itemId}",
          fieldId: "${PARENT_FIELD_ID}",
          value: { itemId: "${parentItemId}" }
        }) { clientMutationId }
      }
    `);
  }

  return itemId;
}

/* ---------------- main ---------------- */

async function run() {
  const repoId = await getRepoId();

  for (const us of backlog.userStories) {
    const usTitle = `[${us.id}] ${us.title}`;

    let usIssue = await findIssueByTitle(usTitle);
    if (!usIssue) {
      usIssue = await createIssue(repoId, usTitle, "User Story");
      console.log("Created US:", usTitle);
    } else {
      console.log("US exists:", usTitle);
    }

    const usItemId = await addToProject(usIssue.id);

    for (const task of us.tasks) {
      const taskTitle = `[${task.id}] ${task.title}`;

      let taskIssue = await findIssueByTitle(taskTitle);
      if (!taskIssue) {
        taskIssue = await createIssue(
          repoId,
          taskTitle,
          `Sub-task of ${us.id}`
        );
        console.log("  Created task:", taskTitle);
      } else {
        console.log("  Task exists:", taskTitle);
      }

      await addToProject(taskIssue.id, usItemId);
      await sleep(600); // анти rate-limit
    }
  }

  console.log("✅ Backlog synced");
}

run().catch(console.error);