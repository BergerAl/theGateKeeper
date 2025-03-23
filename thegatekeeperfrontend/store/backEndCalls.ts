import { createAsyncThunk } from "@reduxjs/toolkit";

export const domainUrlPrefix = () => {
    if (process.env.urlPrefix == undefined) {
        return ''
    } else {
        return process.env.urlPrefix
    }
}

export const voteForUser = createAsyncThunk(
    'theGateKeeper/voteForUser',
    async (userName: string) => {
        const response = await fetch(`${domainUrlPrefix()}/api/TheGateKeeper/voteForUser?userName=${userName}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' }
        })
        if (!response.ok) {
            throw new Error(`Failed voting for user: ${response.statusText}`);
        }
    }
);

export const fetchAllUsers = createAsyncThunk(
    'theGateKeeper/fetchAllUsers',
    async () => {
        const response = await fetch(`${domainUrlPrefix()}/api/TheGateKeeper/getCurrentRanks`);
        if (!response.ok) {
            throw new Error(`Failed to fetch all users: ${response.statusText}`);
        }

        const data = await response.json();
        return data;
    }
);

export const healthCheck = createAsyncThunk(
    'theGateKeeper/healthChecker',
    async () => {
        const response = await fetch(`${domainUrlPrefix()}/api/health`);
        if (!response.ok) {
            throw new Error(`There was an error connecting to Riot Api. Please check if your API key is valid and if the api is reachable`);
        }

        const data = await response.text();
        return data;
    }
); 