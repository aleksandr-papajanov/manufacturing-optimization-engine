import fs from "fs";
import { graphql } from "@octokit/graphql";

// ============================================================================
// Configuration
// ============================================================================

const CONFIG = {
  token: process.env.GITHUB_TOKEN,
  repository: process.env.GITHUB_REPOSITORY,
  projectId: "PVT_kwHOAg280c4BKdGd",
  statusFieldId: "PVTSSF_lAHOAg280c4BKdGdzg6UWKY",
  statusBacklogId: "c463424b",
  sleepMs: 300
};

const LABEL_COLORS = {
  backend: "0E8A16",
  frontend: "1D76DB",
  optimization: "5319E7",
  communication: "FBCA04",
  workflow: "D93F0B",
  data: "0052CC",
  infrastructure: "5A6378",
  testing: "C5DEF5",
  security: "B60205",
  analytics: "006B75",
  monitoring: "E99695",
  documentation: "FEF2C0",
  architecture: "D4C5F9",
  deployment: "BFD4F2"
};

// ============================================================================
// GraphQL Client
// ============================================================================

const graphqlClient = graphql.defaults({
  headers: { authorization: `bearer ${CONFIG.token}` }
});

// ============================================================================
// Data Models
// ============================================================================

const backlog = JSON.parse(fs.readFileSync("backlog.json", "utf8"));

// ============================================================================
// Utilities
// ============================================================================

const sleep = (ms) => new Promise(resolve => setTimeout(resolve, ms));

function parseRepository() {
  const [owner, name] = CONFIG.repository.split("/");
  return { owner, name };
}

// ============================================================================
// GitHub API: Repository
// ============================================================================

async function getRepositoryId() {
  const { owner, name } = parseRepository();
  const query = `
    query ($owner: String!, $name: String!) {
      repository(owner: $owner, name: $name) {
        id
      }
    }
  `;
  
  const result = await graphqlClient(query, { owner, name });
  return result.repository.id;
}

// ============================================================================
// GitHub API: Labels
// ============================================================================

const labelCache = new Set();

async function ensureLabel(labelName) {
  if (labelCache.has(labelName)) return;
  
  const { owner, name } = parseRepository();
  const color = LABEL_COLORS[labelName] || "EDEDED";
  
  try {
    const checkQuery = `
      query ($owner: String!, $name: String!, $labelName: String!) {
        repository(owner: $owner, name: $name) {
          label(name: $labelName) {
            id
          }
        }
      }
    `;
    
    await graphqlClient(checkQuery, { owner, name, labelName });
    labelCache.add(labelName);
  } catch (err) {
    await createLabel(labelName, color);
  }
}

async function createLabel(labelName, color) {
  const repoId = await getRepositoryId();
  const mutation = `
    mutation ($repoId: ID!, $name: String!, $color: String!) {
      createLabel(input: {
        repositoryId: $repoId,
        name: $name,
        color: $color
      }) {
        label { id }
      }
    }
  `;
  
  try {
    await graphqlClient(mutation, { repoId, name: labelName, color });
    labelCache.add(labelName);
    console.log(`    Created label: ${labelName}`);
  } catch (err) {
    console.warn(`    Could not create label ${labelName}:`, err.message);
  }
}

async function getLabelIds(labelNames) {
  const { owner, name } = parseRepository();
  const ids = [];
  
  for (const labelName of labelNames) {
    await ensureLabel(labelName);
    
    try {
      const query = `
        query ($owner: String!, $name: String!, $labelName: String!) {
          repository(owner: $owner, name: $name) {
            label(name: $labelName) {
              id
            }
          }
        }
      `;
      
      const result = await graphqlClient(query, { owner, name, labelName });
      if (result.repository.label) {
        ids.push(result.repository.label.id);
      }
    } catch (err) {
      // Label not found, skip
    }
  }
  
  return ids;
}

// ============================================================================
// GitHub API: Issues
// ============================================================================

async function findIssueByTitle(title) {
  const { owner, name } = parseRepository();
  const query = `
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
  `;
  
  const result = await graphqlClient(query, {
    q: `repo:${owner}/${name} in:title "${title}"`
  });
  
  return result.search.nodes[0] || null;
}

async function createIssue(title, body, labelIds = []) {
  const repoId = await getRepositoryId();
  const mutation = `
    mutation ($input: CreateIssueInput!) {
      createIssue(input: $input) {
        issue {
          id
          title
          number
        }
      }
    }
  `;
  
  const input = {
    repositoryId: repoId,
    title,
    body
  };
  
  if (labelIds.length > 0) {
    input.labelIds = labelIds;
  }
  
  const result = await graphqlClient(mutation, { input });
  return result.createIssue.issue;
}

// ============================================================================
// GitHub API: Sub-Issues
// ============================================================================

async function getSubIssues(issueId) {
  const query = `
    query ($id: ID!) {
      node(id: $id) {
        ... on Issue {
          subIssues(first: 100) {
            nodes {
              id
            }
          }
        }
      }
    }
  `;
  
  const result = await graphqlClient(query, { id: issueId });
  return result.node?.subIssues?.nodes || [];
}

async function linkSubIssue(parentIssueId, subIssueId) {
  const existingSubIssues = await getSubIssues(parentIssueId);
  const alreadyLinked = existingSubIssues.some(sub => sub.id === subIssueId);
  
  if (alreadyLinked) {
    console.log(`      ⏭️  Already linked as sub-issue`);
    return;
  }
  
  const mutation = `
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
  `;
  
  try {
    await graphqlClient(mutation, { issueId: parentIssueId, subIssueId });
    console.log(`      ✅ Linked as sub-issue`);
  } catch (err) {
    if (err.message?.includes("duplicate")) {
      console.log(`      ⏭️  Already linked as sub-issue`);
    } else {
      console.log("      ⚠️ Could not link as sub-issue:", err.message);
    }
  }
}

// ============================================================================
// GitHub API: Projects
// ============================================================================

async function getProjectItemId(issueId) {
  const query = `
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
  `;
  
  try {
    const result = await graphqlClient(query, {
      projectId: CONFIG.projectId,
      contentId: issueId
    });
    return result.node.items.nodes[0]?.id || null;
  } catch (err) {
    return null;
  }
}

async function addIssueToProject(issueId) {
  const mutation = `
    mutation ($projectId: ID!, $contentId: ID!) {
      addProjectV2ItemById(input: {
        projectId: $projectId,
        contentId: $contentId
      }) {
        item { id }
      }
    }
  `;
  
  const result = await graphqlClient(mutation, {
    projectId: CONFIG.projectId,
    contentId: issueId
  });
  
  return result.addProjectV2ItemById.item.id;
}

async function setProjectItemStatus(itemId) {
  const mutation = `
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
  `;
  
  try {
    await graphqlClient(mutation, {
      projectId: CONFIG.projectId,
      itemId,
      fieldId: CONFIG.statusFieldId,
      optionId: CONFIG.statusBacklogId
    });
  } catch (err) {
    console.log("    Could not set status");
  }
}

async function ensureIssueInProject(issueId) {
  let itemId = await getProjectItemId(issueId);
  
  if (itemId) {
    console.log("    Item already in project");
    return itemId;
  }
  
  try {
    itemId = await addIssueToProject(issueId);
    await sleep(CONFIG.sleepMs);
    
    await setProjectItemStatus(itemId);
    await sleep(CONFIG.sleepMs);
    
    return itemId;
  } catch (err) {
    if (err.message?.includes("Content already exists")) {
      console.log("    Item already in project (race condition)");
      return await getProjectItemId(issueId);
    }
    throw err;
  }
}

// ============================================================================
// Business Logic: User Stories
// ============================================================================

async function processUserStory(userStory) {
  const title = `[${userStory.id}] ${userStory.title}`;
  const body = `**Epic:** ${userStory.epic}
    **Priority:** ${userStory.priority}

    ${userStory.userStory}`;

  let issue = await findIssueByTitle(title);
  
  if (!issue) {
    const labelIds = await getLabelIds(userStory.labels);
    issue = await createIssue(title, body, labelIds);
    console.log("✅ Created US:", title);
    await sleep(CONFIG.sleepMs);
  } else {
    console.log("⏭️  US exists:", title);
  }

  await ensureIssueInProject(issue.id);
  await sleep(CONFIG.sleepMs);

  return issue;
}

// ============================================================================
// Business Logic: Tasks
// ============================================================================

async function processTask(task, parentIssue) {
  const title = `[${task.id}] ${task.title}`;
  const body = task.dependsOn.length > 0 
    ? `**Dependencies:** ${task.dependsOn.join(", ")}` 
    : "";

  let issue = await findIssueByTitle(title);
  
  if (!issue) {
    const labelIds = await getLabelIds(task.labels);
    issue = await createIssue(title, body, labelIds);
    console.log(`  ✅ Created task: ${title}`);
    await sleep(CONFIG.sleepMs);
  } else {
    console.log(`  ⏭️  Task exists: ${title}`);
  }
  
  await linkSubIssue(parentIssue.id, issue.id);
  await sleep(CONFIG.sleepMs);

  await ensureIssueInProject(issue.id);
  await sleep(CONFIG.sleepMs);
}

// ============================================================================
// Business Logic: Labels Setup
// ============================================================================

async function setupLabels() {
  console.log("Creating labels...\n");
  
  const allLabels = new Set();
  backlog.userStories.forEach(us => {
    us.labels.forEach(label => allLabels.add(label));
    us.tasks.forEach(task => {
      task.labels.forEach(label => allLabels.add(label));
    });
  });
  
  for (const labelName of allLabels) {
    await ensureLabel(labelName);
  }
  
  console.log("\n✅ Labels ready\n");
}

// ============================================================================
// Main Entry Point
// ============================================================================

async function run() {
  const repoId = await getRepositoryId();
  
  console.log("Repository ID:", repoId);
  console.log("Project ID:", CONFIG.projectId);
  
  await setupLabels();
  
  console.log(`Processing ${backlog.userStories.length} user stories...\n`);

  for (const userStory of backlog.userStories) {
    const usIssue = await processUserStory(userStory);
    
    for (const task of userStory.tasks) {
      await processTask(task, usIssue);
    }
    
    console.log("");
  }

  console.log("🎉 Backlog synced successfully!");
}

// ============================================================================
// Error Handling
// ============================================================================

run().catch(err => {
  console.error("❌ Error:", err.message);
  process.exit(1);
});
