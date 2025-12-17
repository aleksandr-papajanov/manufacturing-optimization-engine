import fs from "fs";
import fetch from "node-fetch";

const token = process.env.GITHUB_TOKEN;
const repo = process.env.GITHUB_REPOSITORY;

const headers = {
    Authorization: `token ${token}`,
    "Content-Type": "application/json"
};

const backlog = JSON.parse(fs.readFileSync("backlog.json", "utf8"));

function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

async function getExistingIssues() {
    const issues = [];
    let page = 1;

    while (true) {
        const res = await fetch(
            `https://api.github.com/repos/${repo}/issues?state=all&per_page=100&page=${page}`,
            { headers }
        );

        const data = await res.json();

        if (!data.length) break;

        issues.push(...data);
        page++;
    }

    return issues;
}

async function createIssue(task) {
    const body = {
        title: `[${task.id}] ${task.title}`,
        body: task.body,
        labels: task.labels || []
    };

    const res = await fetch(`https://api.github.com/repos/${repo}/issues`, {
        method: "POST",
        headers,
        body: JSON.stringify(body)
    });

    if (!res.ok) {
        const text = await res.text();
        throw new Error(`Failed to create issue: ${res.status} ${text}`);
    }

    const data = await res.json();
    console.log(`Created issue: ${data.title}`);
}

async function run() {
    const existing = await getExistingIssues();

    for (const task of backlog.tasks) {
        const title = `[${task.id}] ${task.title}`;
        if (existing.some(issue => issue.title === title)) {
            console.log(`Skipping existing issue: ${title}`);
            continue;
        }
        await createIssue(task);
        await sleep(500); // To avoid hitting rate limits
    }

    console.log("Backlog sync complete!");
}

run().catch(err => console.error(err));