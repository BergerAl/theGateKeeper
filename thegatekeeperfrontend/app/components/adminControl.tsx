import React, { useEffect } from 'react';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import Paper from '@mui/material/Paper';
import SwipeableDrawer from '@mui/material/SwipeableDrawer';
import { Select, MenuItem, InputLabel, FormControl, Box, SelectChangeEvent, Fab, Button } from '@mui/material'
import AddIcon from '@mui/icons-material/Add'
import { useAppDispatch, useAppSelector } from '@/store/hooks';
import { DisplayedView } from '@/store/features/baseComponentsSlice';
import { fetchCurrentVotingStandings, updateConfiguration } from '@/store/backEndCalls';

interface SelectOption {
    value: string
    label: string
}

const AdminControl = () => {
    const dispatch = useAppDispatch()
    const [open, setOpen] = React.useState(false)
    const actualView = useAppSelector(state => state.viewStateSlice.appConfiguration)
    const currentVoting = useAppSelector(state => state.viewStateSlice.votingStandings)
    useEffect(() => {
        dispatch(fetchCurrentVotingStandings())
        // eslint-disable-next-line
    }, []);
    const options: SelectOption[] = (
        Object.keys(DisplayedView)
            .filter(key => isNaN(Number(key)))
            .map(key => ({
                value: DisplayedView[key as keyof typeof DisplayedView],
                label: key
            }))
    )

    const handleSelectChange = (event: SelectChangeEvent<string>) => {
        let clone = { ...actualView }
        clone.displayedView = event.target.value as any as DisplayedView
        dispatch(updateConfiguration(clone))
    }

    return (
        <>
            <Box sx={{ position: 'fixed', bottom: 16, right: 16, zIndex: 100 }}>
                <Fab
                    color="primary"
                    onClick={() => setOpen(true)}
                    sx={{ boxShadow: 3 }}
                >
                    <AddIcon />
                </Fab>
            </Box>
            <SwipeableDrawer
                anchor="right"
                open={open}
                onClose={() => setOpen(false)}
                onOpen={() => setOpen(true)}
                disableBackdropTransition={typeof window !== 'undefined' &&
                    /iPad|iPhone|iPod/.test(window.navigator.userAgent)}
                disableDiscovery={typeof window !== 'undefined' &&
                    /iPad|iPhone|iPod/.test(window.navigator.userAgent)}
            >
                <Box sx={{ width: 250, p: 2 }}>
                    <FormControl fullWidth>
                        <InputLabel>Select an option</InputLabel>
                        <Select
                            value={actualView.displayedView}
                            label="Select an option"
                            onChange={handleSelectChange}
                        >
                            {options.map((option) => (
                                <MenuItem key={option.value} value={option.value}>
                                    {option.label}
                                </MenuItem>
                            ))}
                        </Select>
                    </FormControl>
                </Box>
                <TableContainer component={Paper} >
                    <Table sx={{ minWidth: 200 }} aria-label="simple table">
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
                <Button onPointerDown={() => dispatch(fetchCurrentVotingStandings())} variant="contained">{"Refresh"}</Button>
            </SwipeableDrawer>
        </>
    )
}

export default AdminControl
