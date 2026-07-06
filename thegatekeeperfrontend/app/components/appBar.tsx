import * as React from 'react';
import AppBar from '@mui/material/AppBar';
import Box from '@mui/material/Box';
import Toolbar from '@mui/material/Toolbar';
import IconButton from '@mui/material/IconButton';
import Typography from '@mui/material/Typography';
import Menu from '@mui/material/Menu';
import MenuIcon from '@mui/icons-material/Menu';
import Container from '@mui/material/Container';
import Button from '@mui/material/Button';
import MenuItem from '@mui/material/MenuItem';
import AccountCircle from '@mui/icons-material/AccountCircle';
import NotificationsIcon from '@mui/icons-material/Notifications';
import NotificationsOffIcon from '@mui/icons-material/NotificationsOff';
import CircularProgress from '@mui/material/CircularProgress';
import DownloadIcon from '@mui/icons-material/Download';
import Snackbar from '@mui/material/Snackbar';
import Alert from '@mui/material/Alert';
import { useAppDispatch, useAppSelector } from '@/store/hooks';
import { NavigationTab, setUserNavigation } from '@/store/features/userSlice';
import { domainUrlPrefix } from '@/store/backEndCalls';
import { Tooltip } from '@mui/material';
import ClickAwayListener from '@mui/material/ClickAwayListener';
import { translateUsersOnline } from '../common/common';
import { useAuth } from 'react-oidc-context';
import { usePushNotifications } from '../common/usePushNotifications';

function ResponsiveAppBar() {
    const dispatch = useAppDispatch()
    const [anchorElNav, setAnchorElNav] = React.useState<null | HTMLElement>(null);
    const usersOnline = useAppSelector(state => state.viewStateSlice.appInfo.usersOnline)
    const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);
    const auth = useAuth();
    const accessToken = auth.user?.access_token;
    const { isSubscribed, isLoading: pushLoading, error: pushError, subscribe, unsubscribe } = usePushNotifications(accessToken);
    const [pushErrorOpen, setPushErrorOpen] = React.useState(false);
    const [installPrompt, setInstallPrompt] = React.useState<Event & { prompt: () => Promise<void> } | null>(null);

    React.useEffect(() => {
        const handler = (e: Event) => {
            e.preventDefault();
            setInstallPrompt(e as Event & { prompt: () => Promise<void> });
        };
        window.addEventListener('beforeinstallprompt', handler);
        return () => window.removeEventListener('beforeinstallprompt', handler);
    }, []);

    React.useEffect(() => {
        if (pushError) setPushErrorOpen(true);
    }, [pushError]);

    const handleOpenNavMenu = (event: React.MouseEvent<HTMLElement>) => {
        setAnchorElNav(event.currentTarget);
    };
    const [toolTipOpen, setToolTipOpen] = React.useState(false);

    const handleTooltipClose = () => {
        setToolTipOpen(false);
    };

    const handleTooltipOpen = () => {
        setToolTipOpen(true);
    };
    const handleCloseNavMenu = () => {
        setAnchorElNav(null);
    };

    const handleMenu = (event: React.MouseEvent<HTMLElement>) => {
        setAnchorEl(event.currentTarget);
    };

    const handleClose = () => {
        setAnchorEl(null);
    };

    const enabledTabs = useAppSelector(state => state.viewStateSlice.appConfiguration.enabledTabs);

    const navigationOptions = Object.values(NavigationTab).filter(value => {
        return enabledTabs.includes(value);
    });
    return (
        <>
        <AppBar position="static">
            <Container style={{ maxWidth: '100%' }}>
                <Toolbar disableGutters>
                    {/* <AdbIcon sx={{ display: { xs: 'none', md: 'flex' }, mr: 1 }} /> */}
                    <Typography
                        variant="h6"
                        noWrap
                        component="a"
                        href="/"
                        sx={{
                            mr: 2,
                            display: { xs: 'none', md: 'flex' },
                            fontFamily: 'monospace',
                            fontWeight: 700,
                            letterSpacing: '.3rem',
                            color: 'inherit',
                            textDecoration: 'none',
                        }}
                    >
                        THE GATEKEEPER
                    </Typography>

                    <Box sx={{ flexGrow: 1, display: { xs: 'flex', md: 'none' } }}>
                        <IconButton
                            size="large"
                            aria-label="account of current user"
                            aria-controls="menu-appbar"
                            aria-haspopup="true"
                            onClick={handleOpenNavMenu}
                            color="inherit"
                        >
                            <MenuIcon />
                        </IconButton>
                        <Menu
                            id="menu-appbar"
                            anchorEl={anchorElNav}
                            anchorOrigin={{
                                vertical: 'bottom',
                                horizontal: 'left',
                            }}
                            keepMounted
                            transformOrigin={{
                                vertical: 'top',
                                horizontal: 'left',
                            }}
                            open={Boolean(anchorElNav)}
                            onClose={handleCloseNavMenu}
                            sx={{ display: { xs: 'block', md: 'none' } }}
                        >
                            {navigationOptions.map((option) => (
                                <MenuItem key={option} onPointerDown={() => {
                                    dispatch(setUserNavigation(option as any as NavigationTab))
                                    setAnchorElNav(null);
                                }
                                }>
                                    <Typography sx={{ textAlign: 'center' }}>{option}</Typography>
                                </MenuItem>
                            ))}
                        </Menu>
                    </Box>
                    <ClickAwayListener onClickAway={handleTooltipClose}>
                        <Tooltip title={translateUsersOnline(usersOnline)}
                            onClose={handleTooltipClose}
                            open={toolTipOpen}>
                            <div onClick={handleTooltipOpen} style={{ position: 'relative', display: 'inline-block' }}>
                                <img src={`${domainUrlPrefix()}/images/clown.png`} alt="Clown" width={32} height={32} />
                                <span style={{
                                    position: 'absolute',
                                    bottom: 0,
                                    right: 0,
                                    width: '16px',
                                    height: '16px',
                                    borderRadius: '50%',
                                    border: '1px solid white',
                                    backgroundColor: '#610b0b',
                                    textAlign: 'center',
                                    fontSize: 'smaller'
                                }}>
                                    {usersOnline}
                                </span>
                            </div>
                        </Tooltip>
                    </ClickAwayListener>
                    <Typography
                        variant="h5"
                        noWrap
                        component="a"
                        sx={{
                            mr: 2,
                            display: { xs: 'flex', md: 'none' },
                            flexGrow: 1,
                            fontFamily: 'monospace',
                            fontWeight: 700,
                            letterSpacing: '.3rem',
                            color: 'inherit',
                            textDecoration: 'none',
                        }}
                    >
                        THE GATEKEEPER
                    </Typography>
                    <Box sx={{ flexGrow: 1, display: { xs: 'none', md: 'flex' } }}>
                        {navigationOptions.map((option) => (
                            <Button
                                key={option}
                                sx={{ my: 2, color: 'white', display: 'block' }}
                                onPointerDown={() => {
                                    dispatch(setUserNavigation(option as any as NavigationTab))
                                    setAnchorElNav(null);
                                }
                                }
                            >
                                {option}
                            </Button>
                        ))}
                    </Box>
                    {/* Notification Bell — visible only when logged in */}
                    {auth.isAuthenticated && (
                        <Tooltip title={pushError ?? (isSubscribed ? 'Unsubscribe from notifications' : 'Subscribe to notifications')}>
                            <span>
                                <IconButton
                                    size="large"
                                    color="inherit"
                                    onClick={isSubscribed ? unsubscribe : subscribe}
                                    disabled={pushLoading}
                                    aria-label={isSubscribed ? 'unsubscribe notifications' : 'subscribe notifications'}
                                >
                                    {pushLoading
                                        ? <CircularProgress size={24} color="inherit" />
                                        : isSubscribed
                                            ? <NotificationsIcon />
                                            : <NotificationsOffIcon />}
                                </IconButton>
                            </span>
                        </Tooltip>
                    )}
                    {/* User Button */}
                    <Box sx={{ flexGrow: 0 }}>
                        {auth.isLoading ? (
                            <CircularProgress size={28} color="inherit" sx={{ mx: 1 }} />
                        ) : (
                            <div>
                                <IconButton
                                    size="large"
                                    aria-label="account of current user"
                                    aria-controls="menu-appbar"
                                    aria-haspopup="true"
                                    onClick={handleMenu}
                                    color="inherit"
                                >
                                    <AccountCircle />
                                    <Typography>{auth.user?.profile.name}</Typography>
                                </IconButton>
                                <Menu
                                    id="menu-appbar"
                                    anchorEl={anchorEl}
                                    anchorOrigin={{
                                        vertical: 'top',
                                        horizontal: 'right',
                                    }}
                                    keepMounted
                                    transformOrigin={{
                                        vertical: 'top',
                                        horizontal: 'right',
                                    }}
                                    open={Boolean(anchorEl)}
                                    onClose={handleClose}
                                >
                                    {auth.isAuthenticated ? (
                                        <span>
                                            {installPrompt && (
                                                <MenuItem onClick={() => {
                                                    installPrompt.prompt();
                                                    setInstallPrompt(null);
                                                    handleClose();
                                                }}>
                                                    <DownloadIcon fontSize="small" sx={{ mr: 1 }} />Install app
                                                </MenuItem>
                                            )}
                                            <MenuItem onClick={() => auth.signoutRedirect({ post_logout_redirect_uri: window.location.origin })}>Logout</MenuItem>
                                        </span>
                                    ) : (
                                        <MenuItem onClick={() => auth.signinRedirect()}>Login</MenuItem>
                                    )}
                                </Menu>
                            </div>
                        )}
                    </Box>
                </Toolbar>
            </Container>
        </AppBar>
        <Snackbar
            open={pushErrorOpen}
            autoHideDuration={6000}
            onClose={() => setPushErrorOpen(false)}
            anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
        >
            <Alert severity="error" onClose={() => setPushErrorOpen(false)}>
                {pushError}
            </Alert>
        </Snackbar>
        </>
    );
}
export default ResponsiveAppBar;