'use client';
import React, { useEffect, useState } from 'react';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import Paper from '@mui/material/Paper';
import CircularProgress from '@mui/material/CircularProgress';
import Typography from '@mui/material/Typography';
import Box from '@mui/material/Box';
import { domainUrlPrefix } from '@/store/backEndCalls';

interface VotingResult {
    username: string;
    votes: number;
    votesCast: number;
}

interface Props {
    accessToken: string | undefined;
}

export const VotingResultsTab: React.FC<Props> = ({ accessToken }) => {
    const [results, setResults] = useState<VotingResult[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        if (!accessToken) return;
        setLoading(true);
        fetch(`${domainUrlPrefix()}/api/keycloak/userVotes`, {
            headers: { Authorization: `Bearer ${accessToken}` },
        })
            .then(res => {
                if (!res.ok) throw new Error(`HTTP ${res.status}`);
                return res.json() as Promise<VotingResult[]>;
            })
            .then(data => setResults([...data].sort((a, b) => b.votes - a.votes)))
            .catch(err => setError(err instanceof Error ? err.message : 'Failed to load voting results'))
            .finally(() => setLoading(false));
    }, [accessToken]);

    if (loading) return (
        <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}>
            <CircularProgress />
        </Box>
    );

    if (error) return (
        <Box sx={{ p: 2 }}>
            <Typography color="error">Failed to load voting results: {error}</Typography>
        </Box>
    );

    return (
        <TableContainer component={Paper}>
            <Table aria-label="voting results table">
                <TableHead>
                    <TableRow>
                        <TableCell>#</TableCell>
                        <TableCell>Username</TableCell>
                        <TableCell align="center">Votes</TableCell>
                        <TableCell align="center">Votes Cast</TableCell>
                    </TableRow>
                </TableHead>
                <TableBody>
                    {results.map((result, index) => (
                        <TableRow key={result.username}>
                            <TableCell>{index + 1}</TableCell>
                            <TableCell>{result.username}</TableCell>
                            <TableCell align="center">{result.votes}</TableCell>
                            <TableCell align="center">{result.votesCast}</TableCell>
                        </TableRow>
                    ))}
                </TableBody>
            </Table>
        </TableContainer>
    );
};
