import React from 'react';
import './App.css';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import Paper from '@mui/material/Paper';
import Button from '@mui/material/Button';
import { useAppDispatch, useAppSelector } from '@/store/hooks';
import { voteForUser } from '@/store/backEndCalls';
import { DisplayedView } from '@/store/features/baseComponentsSlice';
import ResponsiveAppBar from './appBar';
import { NavigationTab } from '@/store/features/userSlice';
import { CurrentVoteStandings } from './currentVoteStandings';

export const TheGateKeeper: React.FC = () => {
  const users = useAppSelector(state => state.viewStateSlice.frontEndInfo)
  const appConfig = useAppSelector(state => state.viewStateSlice.appConfiguration)
  const gateKeeperName = useAppSelector(state => state.viewStateSlice.gateKeeperInfo.name)
  const userNavigation = useAppSelector(state => state.userSlice.currentNavigation)
  const dispatch = useAppDispatch()
  return (
    <>
      <ResponsiveAppBar />
      {appConfig.displayedView == DisplayedView.DefaultPage && userNavigation == NavigationTab.ActualTable &&
        <TableContainer component={Paper} >
          <Table sx={{ minWidth: 650 }} aria-label="simple table">
            <TableHead>
              <TableRow>
                <TableCell>Name</TableCell>
                <TableCell align="right">Tier</TableCell>
                <TableCell align="right">Rank</TableCell>
                <TableCell align="right">LeaguePoints</TableCell>
                <TableCell align="right">Played Games</TableCell>
                <TableCell align="right">VOTE NOW!</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {users.map((row) => (
                <TableRow
                  key={row.name}
                  sx={{ '&:last-child td, &:last-child th': { border: 0 }, backgroundColor: row.name == gateKeeperName ? "#FF474C" : "default" }}
                >
                  <TableCell component="th" scope="row">
                    {row.name}
                  </TableCell>
                  <TableCell align="right">{row.tier}</TableCell>
                  <TableCell align="right">{row.rank}</TableCell>
                  <TableCell align="right">{row.leaguePoints}</TableCell>
                  <TableCell align="right">{row.playedGames}</TableCell>
                  <TableCell align="right"><Button onPointerDown={() => dispatch(voteForUser(row.name))} disabled={row.voting.isBlocked || appConfig.votingDisabled} variant="contained">{"Vote"}</Button></TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>}
      {appConfig.displayedView == DisplayedView.DefaultPage && userNavigation == NavigationTab.VotingStandings &&
        <CurrentVoteStandings />}
      {/* TODO: Implement result page */}
      {appConfig.displayedView == DisplayedView.ResultsPage && <div>{DisplayedView.ResultsPage}</div>}
    </>
  )
}
