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

interface KeycloakUser {
    username: string;
    firstName: string | null;
    lastName: string | null;
}

interface Props {
    accessToken: string | undefined;
}

export const KeycloakUsersTab: React.FC<Props> = ({ accessToken }) => {
    const [users, setUsers] = useState<KeycloakUser[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        if (!accessToken) return;
        setLoading(true);
        fetch(`${domainUrlPrefix()}/api/keycloak/users`, {
            headers: { Authorization: `Bearer ${accessToken}` },
        })
            .then(res => {
                if (!res.ok) throw new Error(`HTTP ${res.status}`);
                return res.json() as Promise<KeycloakUser[]>;
            })
            .then(data => setUsers(data))
            .catch(err => setError(err instanceof Error ? err.message : 'Failed to load users'))
            .finally(() => setLoading(false));
    }, [accessToken]);

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
        <TableContainer component={Paper}>
            <Table aria-label="keycloak users table">
                <TableHead>
                    <TableRow>
                        <TableCell>Username</TableCell>
                        <TableCell>First Name</TableCell>
                        <TableCell>Last Name</TableCell>
                    </TableRow>
                </TableHead>
                <TableBody>
                    {users.map(user => (
                        <TableRow key={user.username}>
                            <TableCell>{user.username}</TableCell>
                            <TableCell>{user.firstName ?? '—'}</TableCell>
                            <TableCell>{user.lastName ?? '—'}</TableCell>
                        </TableRow>
                    ))}
                </TableBody>
            </Table>
        </TableContainer>
    );
};
