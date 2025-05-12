import React, { useEffect } from 'react';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import Paper from '@mui/material/Paper';
import { useAppDispatch, useAppSelector } from '@/store/hooks';
import { fetchCurrentVoteStandings } from '@/store/backEndCalls';

export const CurrentVoteStandings: React.FC = () => {
    const dispatch = useAppDispatch()
    const currentVoting = useAppSelector(state => state.viewStateSlice.voteStandings)
    useEffect(() => {
        dispatch(fetchCurrentVoteStandings())
        // eslint-disable-next-line
    }, []);
    return (
        <TableContainer component={Paper} >
            <Table aria-label="simple table">
                <TableHead>
                    <TableRow>
                        <TableCell>Name</TableCell>
                        <TableCell align="right">Votes</TableCell>
                    </TableRow>
                </TableHead>
                <TableBody>
                    {currentVoting.map((row) => (
                        <TableRow
                            key={row.playerName}
                        >
                            <TableCell align="left">{row.playerName}</TableCell>
                            <TableCell align="right">{row.votes}</TableCell>
                        </TableRow>
                    ))}
                </TableBody>
            </Table>
        </TableContainer>
    )
};