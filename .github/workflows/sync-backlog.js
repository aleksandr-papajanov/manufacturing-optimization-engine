import fs from "fs";
import { graphql } from "@octokit/graphql";

const token = process.env.GITHUB_TOKEN;
const repoFull = process.env.GITHUB_REPOSITORY;

const PROJECT_ID = "PVT_kwHOAg280c4BKdGd";
const STATUS_FIELD_ID = "PVTSSF_lAHOAg280c4BKdGdzg6UWKY";
const STATUS_BACKLOG_ID = "c463424b";
const PARENT_FIELD_ID = "PVTF_lAHOAg280c4BKdGdzg6UWKw";

const graphqlWithAuth = graphql.defaults({
  headers: { authorization: `bearer ${token}` }
});

const backlog = JSON.parse(fs.readFileSync("backlog.json", "utf8"));

/* ---------------- helpers ---------------- */

const sleep = ms => new Promise(r => setTimeout(r, ms));

const LABEL_COLORS = {
  "backend": "0E8A16",
  "frontend": "1D76DB",
  "optimization": "5319E7",
  "communication": "FBCA04",
  "workflow": "D93F0B",
  "data": "0052CC",
  "infrastructure": "5A6378",
  "testing": "C5DEF5",
  "security": "B60205",
  "analytics": "006B75",
  "monitoring": "E99695",
  "documentation": "FEF2C0",
  "architecture": "D4C5F9",
  "deployment": "BFD4F2"
};

const createdLabels = new Set();

async function ensureLabel(owner, name, labelName) {
  if (createdLabels.has(labelName)) return;
  
  try {
    await graphqlWithAuth(`
      query ($owner: String!, $name: String!, $labelName: String!) {
        repository(owner: $owner, name: $name) {
          label(name: $labelName) {
            id
          }
        }
      }
    `, { owner, name, labelName });
    createdLabels.add(labelName);
  } catch (err) {
    // Label doesn't exist, create it
    try {
      const color = LABEL_COLORS[labelName] || "EDEDED";
      await graphqlWithAuth(`
        mutation ($repoId: ID!, $name: String!, $color: String!) {
          createLabel(input: {
            repositoryId: $repoId,
            name: $name,
            color: $color
          }) {
            label { id }
          }
        }
      `, { repoId: await getRepoId(), name: labelName, color });
      createdLabels.add(labelName);
      console.log(`    Created label: ${labelName}`);
    } catch (e) {
      console.warn(`    Could not create label ${labelName}:`, e.message);
    }
  }
}

async function getLabelIds(owner, name, labelNames) {
  const ids = [];
  for (const labelName of labelNames) {
    await ensureLabel(owner, name, labelName);
    try {
      const res = await graphqlWithAuth(`
        query ($owner: String!, $name: String!, $labelName: String!) {
          repository(owner: $owner, name: $name) {
            label(name: $labelName) {
              id
            }
          }
        }
      `, { owner, name, labelName });
      if (res.repository.label) {
        ids.push(res.repository.label.id);
      }
    } catch (err) {
      // Ignore if label not found
    }
  }
  return ids;
}

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
    query ($q: String!) {
      search(type: ISSUE, query: $q, first: 1) {
        nodes {
          ... on Issue {
            id
            title
            number
          }
        }
      }
    }
  `, {
    q: `repo:${owner}/${name} in:title "${title}"`
  });

  return res.search.nodes[0] ?? null;
}

async function createIssue(repoId, title, body, labelIds = []) {
  const input = {
    repositoryId: repoId,
    title: title,
    body: body
  };
  
  if (labelIds.length > 0) {
    input.labelIds = labelIds;
  }
  
  const res = await graphqlWithAuth(`
    mutation ($input: CreateIssueInput!) {
      createIssue(input: $input) {
        issue { id title number }
      }
    }
  `, { input });

  return res.createIssue.issue;
}

async function addSubIssue(parentIssueId, subIssueId) {
  // Use GitHub's addSubIssue mutation to create proper sub-issue relationship
  try {
    await graphqlWithAuth(`
      mutation ($issueId: ID!, $subIssueId: ID!) {
        addSubIssue(input: {
          issueId: $issueId,
          subIssueId: $subIssueId
        }) {
          issue {
            id
            title
          }
        }
      }
    `, { issueId: parentIssueId, subIssueId: subIssueId });
    
    console.log(`      ✅ Linked as sub-issue`);
  } catch (err) {
    console.log("      ⚠️ Could not link as sub-issue:", err.message);
  }
}

async function getProjectItemId(issueId) {
  try {
    const res = await graphqlWithAuth(`
      query ($projectId: ID!, $contentId: ID!) {
        node(id: $projectId) {
          ... on ProjectV2 {
            items(first: 1, filterBy: { contentId: $contentId }) {
              nodes {
                id
              }
            }
          }
        }
      }
    `, { projectId: PROJECT_ID, contentId: issueId });
    
    return res.node.items.nodes[0]?.id ?? null;
  } catch (err) {
    return null;
  }
}

async function addToProject(issueId, parentItemId = null) {
  // Check if already in project
  let itemId = await getProjectItemId(issueId);
  
  if (!itemId) {
    try {
      const res = await graphqlWithAuth(`
        mutation ($projectId: ID!, $contentId: ID!) {
          addProjectV2ItemById(input: {
            projectId: $projectId,
            contentId: $contentId
          }) {
            item { id }
          }
        }
      `, { projectId: PROJECT_ID, contentId: issueId });

      itemId = res.addProjectV2ItemById.item.id;
      await sleep(300);

      // Status = Backlog
      try {
        await graphqlWithAuth(`
          mutation ($projectId: ID!, $itemId: ID!, $fieldId: ID!, $optionId: String!) {
            updateProjectV2ItemFieldValue(input: {
              projectId: $projectId,
              itemId: $itemId,
              fieldId: $fieldId,
              value: { singleSelectOptionId: $optionId }
            }) { 
              projectV2Item { id }
            }
          }
        `, {
          projectId: PROJECT_ID,
          itemId: itemId,
          fieldId: STATUS_FIELD_ID,
          optionId: STATUS_BACKLOG_ID
        });
      } catch (statusErr) {
        console.log("    Could not set status (field may not exist)");
      }

      await sleep(300);
    } catch (err) {
      if (err.message && err.message.includes("Content already exists")) {
        console.log("    Item already in project (race condition)");
        itemId = await getProjectItemId(issueId);
      } else {
        throw err;
      }
    }
  } else {
    console.log("    Item already in project");
  }

  // Note: Parent link field is read-only in GitHub Projects API
  // Sub-issue relationships are established via GitHub issue dependencies

  return itemId;
}

/* ---------------- main ---------------- */

async function addLabelsToIssue(issueId, labelIds) {
  if (labelIds.length === 0) return;
  
  try {
    await graphqlWithAuth(`
      mutation ($id: ID!, $labelIds: [ID!]!) {
        addLabelsToLabelable(input: {
          labelableId: $id,
          labelIds: $labelIds
        }) {
          labelable { ... on Issue { id } }
        }
      }
    `, { id: issueId, labelIds });
  } catch (err) {
    console.log("      Could not add labels:", err.message);
  }
}

async function run() {
  const repoId = await getRepoId();
  const [owner, name] = repoFull.split("/");
  
  console.log("Repository ID:", repoId);
  console.log("Project ID:", PROJECT_ID);
  console.log("Creating labels...\n");
  
  // Pre-create all labels
  const allLabels = new Set();
  backlog.userStories.forEach(us => {
    us.labels.forEach(l => allLabels.add(l));
    us.tasks.forEach(t => t.labels.forEach(l => allLabels.add(l)));
  });
  
  for (const labelName of allLabels) {
    await ensureLabel(owner, name, labelName);
  }
  
  console.log(`\n✅ Labels ready\n`);
  console.log(`Processing ${backlog.userStories.length} user stories...\n`);

  for (const us of backlog.userStories) {
    const usTitle = `[${us.id}] ${us.title}`;
    const usBody = `**Epic:** ${us.epic}
**Priority:** ${us.priority}

${us.userStory}`;

    let usIssue = await findIssueByTitle(usTitle);
    let isNewUS = false;
    if (!usIssue) {
      const labelIds = await getLabelIds(owner, name, us.labels);
      usIssue = await createIssue(repoId, usTitle, usBody, labelIds);
      console.log("✅ Created US:", usTitle);
      isNewUS = true;
      await sleep(1000);
    } else {
      console.log("⏭️  US exists:", usTitle);
      // Add labels to existing issue
      const labelIds = await getLabelIds(owner, name, us.labels);
      await addLabelsToIssue(usIssue.id, labelIds);
    }

    const usItemId = await addToProject(usIssue.id);
    await sleep(800);

    for (const task of us.tasks) {
      const taskTitle = `[${task.id}] ${task.title}`;
      const taskBody = task.dependsOn.length > 0 ? `**Dependencies:** ${task.dependsOn.join(", ")}` : "";

      let taskIssue = await findIssueByTitle(taskTitle);
      let isNewTask = false;
      if (!taskIssue) {
        const labelIds = await getLabelIds(owner, name, task.labels);
        taskIssue = await createIssue(repoId, taskTitle, taskBody, labelIds);
        console.log(`  ✅ Created task: ${taskTitle}`);
        isNewTask = true;
        await sleep(800);
      } else {
        console.log(`  ⏭️  Task exists: ${taskTitle}`);
        // Add labels to existing task
        const labelIds = await getLabelIds(owner, name, task.labels);
        await addLabelsToIssue(taskIssue.id, labelIds);
      }
      
      // Add as sub-issue to parent (both new and existing)
      await addSubIssue(usIssue.id, taskIssue.id);
      await sleep(500);

      await addToProject(taskIssue.id, usItemId);
      await sleep(800);
    }

    console.log("");
  }

  console.log("🎉 Backlog synced successfully!");
}

run().catch(console.error);