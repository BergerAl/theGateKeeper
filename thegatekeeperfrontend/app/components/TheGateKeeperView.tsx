import React from 'react';
import './App.css';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import Paper from '@mui/material/Paper';
import { useAppSelector } from '@/store/hooks';

function createUserData(
  name: string,
  rank: string,
  tier: string,
  leaguePoints: number,
) {
  return { name, rank, tier, leaguePoints };
}

export const TheGateKeeper: React.FC = () => {
  const users = useAppSelector(state => state.viewStateSlice.frontEndInfo)
  return (

    <TableContainer component={Paper}>
      <Table sx={{ minWidth: 650 }} aria-label="simple table">
        <TableHead>
          <TableRow>
            <TableCell>Name</TableCell>
            <TableCell align="right">Tier</TableCell>
            <TableCell align="right">Rank</TableCell>
            <TableCell align="right">LeaguePoints</TableCell>
            <TableCell align="right">playedGames</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {users.map((row) => (
            <TableRow
              key={row.name}
              sx={{ '&:last-child td, &:last-child th': { border: 0 }, backgroundColor: row.name == "Knechter" ? "#FF474C": "default" }}
            >
              <TableCell component="th" scope="row">
                {row.name}
              </TableCell>
              <TableCell align="right">{row.tier}</TableCell>
              <TableCell align="right">{row.rank}</TableCell>
              <TableCell align="right">{row.leaguePoints}</TableCell>
              <TableCell align="right">{row.playedGames}</TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  )
}
