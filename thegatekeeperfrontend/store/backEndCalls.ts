import { createAsyncThunk } from "@reduxjs/toolkit";
import { AppConfigurationDtoV1 } from "./features/baseComponentsSlice";
export const domainUrlPrefix = () => {
    if (process.env.urlPrefix == undefined) {
        return ''
    } else {
        return process.env.urlPrefix
    }
}

export const adminAccess = () => {
    //This is just a workaround as long as we don't have a user management 
    if (process.env.adminAccess == "true") {
        return true
    }
    return false;
}

export const voteForUser = createAsyncThunk(
    'theGateKeeper/voteForUser',
    async (userName: string) => {
        const response = await fetch(`${domainUrlPrefix()}/api/TheGateKeeper/voteForUser`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(userName)
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

export const fetchConfiguration = createAsyncThunk(
    'theGateKeeper/appConfig',
    async () => {
        const response = await fetch(`${domainUrlPrefix()}/api/AppConfiguration/getConfiguration`);
        if (!response.ok) {
            throw new Error(`Failed to fetch app configuration: ${response.statusText}`);
        }

        const data = await response.json();
        return data;
    }
);

export const updateConfiguration = createAsyncThunk(
    'theGateKeeper/setAppConfig',
    async (appConfig: AppConfigurationDtoV1) => {
        const response = await fetch(`${domainUrlPrefix()}/api/AppConfiguration`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(appConfig)
        })
        if (!response.ok) {
            throw new Error(`Failed to set configuration: ${response.statusText}`);
        }
    }
);

export const fetchCurrentVoteStandings = createAsyncThunk(
    'theGateKeeper/fetchCurrentStandings',
    async () => {
        const response = await fetch(`${domainUrlPrefix()}/api/TheGateKeeper/getCurrentVoteStandings`);
        if (!response.ok) {
            throw new Error(`Failed to fetch all users: ${response.statusText}`);
        }

        const data = await response.json();
        return data;
    }
);

export const fetchGateKeeperInfo = createAsyncThunk(
    'theGateKeeper/fetchGateKeeperInfo',
    async () => {
        const response = await fetch(`${domainUrlPrefix()}/api/AppConfiguration/getGateKeeperInfo`);
        if (!response.ok) {
            throw new Error(`Failed to fetch the gateKeeper infos: ${response.statusText}`);
        }

        const data = await response.json();
        return data;
    }
);