import React, { useEffect } from 'react';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import Paper from '@mui/material/Paper';
import SwipeableDrawer from '@mui/material/SwipeableDrawer';
import { Select, MenuItem, InputLabel, FormControl, Box, SelectChangeEvent, Fab, Button, TextField, FormGroup, FormControlLabel, Checkbox, Typography, Switch } from '@mui/material'
import AddIcon from '@mui/icons-material/Add'
import { useAppDispatch, useAppSelector } from '@/store/hooks';
import { AppConfigurationDtoV1, DisplayedView } from '../../types';
import { fetchCurrentVoteStandings, updateConfiguration, domainUrlPrefix } from '@/store/backEndCalls';
import { useAuth } from 'react-oidc-context';
import { NavigationTab } from '@/store/features/userSlice';
interface SelectOption {
    value: string
    label: string
}

const AdminControl = () => {
    const dispatch = useAppDispatch()
    const [open, setOpen] = React.useState(false)
    const actualConfig = useAppSelector(state => state.viewStateSlice.appConfiguration)
    const currentVoting = useAppSelector(state => state.viewStateSlice.voteStandings)
    const [appConfig, setAppConfig] = React.useState<AppConfigurationDtoV1>(actualConfig)
    const [timerMinutes, setTimerMinutes] = React.useState(0);
    const auth = useAuth();
    const [broadcastTitle, setBroadcastTitle] = React.useState('The GateKeeper');
    const [broadcastBody, setBroadcastBody] = React.useState('');
    const [broadcasting, setBroadcasting] = React.useState(false);
    const [resettingVotes, setResettingVotes] = React.useState(false);

    // Check for Admin role in access token
    // Option A: uses a custom realm role named 'Admin' assigned to the user in Keycloak
    // Option B: uses the built-in 'realm-admin' role from the realm-management client
    const hasAdminRole = (() => {
        if (auth.user?.access_token) {
            try {
                const base64url = auth.user.access_token.split('.')[1]
                    .replace(/-/g, '+').replace(/_/g, '/');
                const payload = JSON.parse(atob(base64url));

                if (Array.isArray(payload.realm_access?.roles) && payload.realm_access.roles.includes('Admin')) return true;

                const realmMgmt = payload.resource_access?.['realm-management'];
                if (Array.isArray(realmMgmt?.roles) && realmMgmt.roles.includes('realm-admin')) return true;
            } catch {
                return false;
            }
        }
        return false;
    })();

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
        setTimerMinutes(0)
        // eslint-disable-next-line
    }, [actualConfig]);

    const handleSelectChange = (event: SelectChangeEvent<string>) => {
        setAppConfig(currentConfig => {
            let newConfig = { ...currentConfig }
            newConfig.displayedView = event.target.value as any as DisplayedView
            return newConfig
        })
    }

    const handleVotingToggle = (event: React.ChangeEvent<HTMLInputElement>) => {
        const enabled = event.target.checked;
        setAppConfig(currentConfig => ({
            ...currentConfig,
            votingDisabled: !enabled,
        }));
        if (!enabled) setTimerMinutes(0);
    };

    const configurableTabs = Object.values(NavigationTab);

    const handleResetKeycloakVotes = async () => {
        setResettingVotes(true);
        try {
            await fetch(`${domainUrlPrefix()}/api/keycloak/userVotes/reset`, {
                method: 'POST',
                headers: { Authorization: `Bearer ${auth.user?.access_token}` },
            });
        } finally {
            setResettingVotes(false);
        }
    };

    const handleTabToggle = (tab: string) => {
        setAppConfig(currentConfig => {
            const enabled = currentConfig.enabledTabs ?? [];
            const newTabs = enabled.includes(tab)
                ? enabled.filter(t => t !== tab)
                : [...enabled, tab];
            return { ...currentConfig, enabledTabs: newTabs };
        });
    };

    const handleBroadcast = async () => {
        if (!broadcastTitle || !broadcastBody) return;
        setBroadcasting(true);
        try {
            await fetch('/api/notifications/broadcast', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    Authorization: `Bearer ${auth.user?.access_token}`,
                },
                body: JSON.stringify({ title: broadcastTitle, body: broadcastBody }),
            });
        } finally {
            setBroadcasting(false);
        }
    };

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
                    <Box sx={{ mt: 1 }}>
                        <FormControlLabel
                            control={
                                <Switch
                                    checked={!appConfig.votingDisabled}
                                    onChange={handleVotingToggle}
                                />
                            }
                            label={appConfig.votingDisabled ? 'Voting disabled' : 'Voting enabled'}
                        />
                        {!appConfig.votingDisabled && (
                            <>
                                <FormControl fullWidth size="small" sx={{ mt: 1 }}>
                                    <InputLabel>Auto-disable after</InputLabel>
                                    <Select
                                        value={timerMinutes}
                                        label="Auto-disable after"
                                        onChange={e => setTimerMinutes(Number(e.target.value))}
                                    >
                                        <MenuItem value={0}>No timer</MenuItem>
                                        <MenuItem value={5}>5 minutes</MenuItem>
                                        <MenuItem value={10}>10 minutes</MenuItem>
                                        <MenuItem value={15}>15 minutes</MenuItem>
                                        <MenuItem value={30}>30 minutes</MenuItem>
                                        <MenuItem value={60}>1 hour</MenuItem>
                                        <MenuItem value={120}>2 hours</MenuItem>
                                        <MenuItem value={240}>4 hours</MenuItem>
                                    </Select>
                                </FormControl>
                                {actualConfig.votingEndsAt && (
                                    <Typography variant="caption" color="success.main" sx={{ mt: 0.5, display: 'block' }}>
                                        Ends at: {new Date(actualConfig.votingEndsAt).toLocaleTimeString()}
                                    </Typography>
                                )}
                            </>
                        )}
                    </Box>
                    <Box sx={{ mt: 1 }}>
                        <Typography variant="caption" color="text.secondary">Visible tabs</Typography>
                        <FormGroup>
                            {configurableTabs.map(tab => (
                                <FormControlLabel
                                    key={tab}
                                    control={
                                        <Checkbox
                                            checked={(appConfig.enabledTabs ?? []).includes(tab)}
                                            onChange={() => handleTabToggle(tab)}
                                            size="small"
                                        />
                                    }
                                    label={tab}
                                />
                            ))}
                        </FormGroup>
                    </Box>
                </Box>
                {/* <TableContainer component={Paper} >
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
                <Button onPointerDown={() => dispatch(fetchCurrentVoteStandings())} variant="contained">{"Refresh"}</Button> */}
                <Button
                    onPointerDown={() => {
                        const configToSave = {
                            ...appConfig,
                            votingEndsAt: appConfig.votingDisabled
                                ? null
                                : timerMinutes > 0
                                    ? new Date(Date.now() + timerMinutes * 60000).toISOString()
                                    : appConfig.votingEndsAt
                        };
                        dispatch(updateConfiguration({ appConfig: configToSave as AppConfigurationDtoV1, token: auth.user?.access_token }));
                    }}
                    variant="contained"
                >{"Save config"}</Button>
                <Button
                    variant="contained"
                    color="error"
                    disabled={resettingVotes}
                    onPointerDown={handleResetKeycloakVotes}
                    sx={{ mt: 1 }}
                >
                    {resettingVotes ? 'Resetting...' : 'Reset Keycloak votes'}
                </Button>
                <Box sx={{ p: 2, display: 'flex', flexDirection: 'column', gap: 1 }}>
                    <TextField
                        label="Notification title"
                        size="small"
                        value={broadcastTitle}
                        onChange={e => setBroadcastTitle(e.target.value)}
                    />
                    <TextField
                        label="Message"
                        size="small"
                        multiline
                        rows={2}
                        value={broadcastBody}
                        onChange={e => setBroadcastBody(e.target.value)}
                    />
                    <Button
                        variant="contained"
                        color="warning"
                        disabled={broadcasting || !broadcastBody}
                        onPointerDown={handleBroadcast}
                    >
                        {broadcasting ? 'Sending...' : 'Send push to all'}
                    </Button>
                </Box>
            </SwipeableDrawer>
        </>
    )
}

export default AdminControl
