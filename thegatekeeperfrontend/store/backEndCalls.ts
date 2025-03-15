import { createAsyncThunk } from "@reduxjs/toolkit";

export const voteForUser = createAsyncThunk(
    'theGateKeeper/voteForUser',
    async (userName: string) => {
        const response = await fetch(`/api/TheGateKeeper/voteForUser?userName=${userName}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' }
        })
        if (!response.ok) {
            throw new Error(`Failed to fetch all available cycles: ${response.statusText}`);
        }
    }
);

export const fetchAllUsers = createAsyncThunk(
    'theGateKeeper/fetchAllUsers',
    async () => {
        const response = await fetch('/api/TheGateKeeper/getCurrentRanks');
        if (!response.ok) {
            throw new Error(`Failed to fetch all available cycles: ${response.statusText}`);
        }

        const data = await response.json();
        return data;
    }
);