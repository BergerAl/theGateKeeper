'use client';
import React, { useCallback, useEffect, useState } from 'react';
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
import Button from '@mui/material/Button';
import Snackbar from '@mui/material/Snackbar';
import Alert from '@mui/material/Alert';
import { domainUrlPrefix } from '@/store/backEndCalls';
import { useAppSelector } from '@/store/hooks';

interface KeycloakUser {
    username: string;
    firstName: string | null;
    lastName: string | null;
}

interface KeycloakUserVote {
    username: string;
    votes: number;
}

interface Props {
    accessToken: string | undefined;
}

export const KeycloakUsersTab: React.FC<Props> = ({ accessToken }) => {
    const votingDisabled = useAppSelector(state => state.viewStateSlice.appConfiguration.votingDisabled);
    const [users, setUsers] = useState<KeycloakUser[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [votingFor, setVotingFor] = useState<string | null>(null);
    const [snackbar, setSnackbar] = useState<{ open: boolean; message: string; severity: 'success' | 'error' }>({ open: false, message: '', severity: 'success' });

    useEffect(() => {
        if (!accessToken) return;
        setLoading(true);
        Promise.all([
            fetch(`${domainUrlPrefix()}/api/keycloak/users`, {
                headers: { Authorization: `Bearer ${accessToken}` },
            }).then(res => {
                if (!res.ok) throw new Error(`HTTP ${res.status}`);
                return res.json() as Promise<KeycloakUser[]>;
            }),
        ])
            .then(([usersData]) => {
                setUsers(usersData);
            })
            .catch(err => setError(err instanceof Error ? err.message : 'Failed to load users'))
            .finally(() => setLoading(false));
    }, [accessToken]);

    const handleVote = async (username: string) => {
        if (!accessToken) return;
        setVotingFor(username);
        try {
            const res = await fetch(`${domainUrlPrefix()}/api/keycloak/users/${encodeURIComponent(username)}/vote`, {
                method: 'POST',
                headers: { Authorization: `Bearer ${accessToken}` },
            });
            if (res.ok) {
                setSnackbar({ open: true, message: `Voted for ${username}!`, severity: 'success' });
            } else {
                const body = await res.json().catch(() => ({ message: `HTTP ${res.status}` }));
                setSnackbar({ open: true, message: body?.message ?? 'Vote failed', severity: 'error' });
            }
        } catch {
            setSnackbar({ open: true, message: 'Vote request failed', severity: 'error' });
        } finally {
            setVotingFor(null);
        }
    };

    if (loading) return (
        <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}>
            <CircularProgress />
        </Box>
    );

    if (error) return (
        <Box sx={{ p: 2 }}>
            <Typography color="error">Failed to load users: {error}</Typography>
        </Box>
    );

    return (
        <>
            <TableContainer component={Paper}>
                <Table aria-label="keycloak users table">
                    <TableHead>
                        <TableRow>
                            <TableCell>Username</TableCell>
                            <TableCell>First Name</TableCell>
                            <TableCell>Last Name</TableCell>
                            <TableCell align="center">Vote</TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {users.map(user => (
                            <TableRow key={user.username}>
                                <TableCell>{user.username}</TableCell>
                                <TableCell>{user.firstName ?? '—'}</TableCell>
                                <TableCell>{user.lastName ?? '—'}</TableCell>
                                <TableCell align="center">
                                    <Button
                                        variant="contained"
                                        size="small"
                                        onClick={() => handleVote(user.username)}
                                        disabled={!accessToken || votingDisabled || votingFor === user.username}
                                    >
                                        {votingFor === user.username ? <CircularProgress size={18} /> : '👍 Vote'}
                                    </Button>
                                </TableCell>
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            </TableContainer>
            <Snackbar
                open={snackbar.open}
                autoHideDuration={4000}
                onClose={() => setSnackbar(s => ({ ...s, open: false }))}
            >
                <Alert severity={snackbar.severity} onClose={() => setSnackbar(s => ({ ...s, open: false }))}>
                    {snackbar.message}
                </Alert>
            </Snackbar>
        </>
    );
};
