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
import { AppConfigurationDtoV1, DisplayedView } from '@/store/features/baseComponentsSlice';
import { fetchCurrentVoteStandings, updateConfiguration } from '@/store/backEndCalls';
import { useAuth } from 'react-oidc-context';
interface SelectOption {
    value: string
    label: string
}

function stringToBoolean(str: string): boolean {
    return str.toLowerCase() === 'true';
}

const AdminControl = () => {
    const dispatch = useAppDispatch()
    const [open, setOpen] = React.useState(false)
    const actualConfig = useAppSelector(state => state.viewStateSlice.appConfiguration)
    const currentVoting = useAppSelector(state => state.viewStateSlice.voteStandings)
    const [appConfig, setAppConfig] = React.useState<AppConfigurationDtoV1>(actualConfig)
    const auth = useAuth();

    // Check for Admin role in access token
    const hasAdminRole = (Array.isArray(auth.user?.profile?.roles) && auth.user.profile.roles.includes('Admin')) ||
        (auth.user?.access_token && (() => {
            try {
                const payload = JSON.parse(atob(auth.user.access_token.split('.')[1]));
                // Check both realm_access and resource_access
                return (Array.isArray(payload.realm_access?.roles) && payload.realm_access.roles.includes('Admin')) ||
                    Object.values(payload.resource_access || {}).some((r: any) => Array.isArray(r.roles) && r.roles.includes('Admin'));
            } catch {
                return false;
            }
        })());

    useEffect(() => {
        dispatch(fetchCurrentVoteStandings())
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

    useEffect(() => {
        setAppConfig(actualConfig)
        // eslint-disable-next-line
    }, [actualConfig]);

    const handleSelectChange = (event: SelectChangeEvent<string>) => {
        setAppConfig(currentConfig => {
            let newConfig = { ...currentConfig }
            newConfig.displayedView = event.target.value as any as DisplayedView
            return newConfig
        })
    }

    const handleVotingChanged = (event: SelectChangeEvent<string>) => {
        setAppConfig(currentConfig => {
            let newConfig = { ...currentConfig }
            newConfig.votingDisabled = stringToBoolean(event.target.value)
            return newConfig
        })
    }

    const handleResultsBarChange = (event: SelectChangeEvent<string>) => {
        setAppConfig(currentConfig => {
            let newConfig = { ...currentConfig }
            newConfig.displayResultsBar = stringToBoolean(event.target.value)
            return newConfig
        })
    }

    if (!hasAdminRole) return null;

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
                        <InputLabel>Select an display option</InputLabel>
                        <Select
                            value={appConfig.displayedView}
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
                    <FormControl fullWidth>
                        <InputLabel>Disable voting mechanism</InputLabel>
                        <Select
                            value={appConfig.votingDisabled as any as string}
                            label="Select an option"
                            onChange={handleVotingChanged}
                        >
                            <MenuItem key={"false"} value={"false"}>
                                {"false"}
                            </MenuItem>
                            <MenuItem key={"true"} value={"true"}>
                                {"true"}
                            </MenuItem>
                        </Select>
                    </FormControl>
                    <FormControl fullWidth>
                        <InputLabel>Display Result Bar</InputLabel>
                        <Select
                            value={appConfig.displayResultsBar as any as string}
                            label="Select an option"
                            onChange={handleResultsBarChange}
                        >
                            <MenuItem key={"false"} value={"false"}>
                                {"false"}
                            </MenuItem>
                            <MenuItem key={"true"} value={"true"}>
                                {"true"}
                            </MenuItem>
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
                <Button onPointerDown={() => dispatch(fetchCurrentVoteStandings())} variant="contained">{"Refresh"}</Button>
                <Button onPointerDown={() => dispatch(updateConfiguration({ appConfig, token: auth.user?.access_token }))} variant="contained">{"Save config"}</Button>
            </SwipeableDrawer>
        </>
    )
}

export default AdminControl
