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
import { useAppDispatch, useAppSelector } from '@/store/hooks';
import { NavigationTab, setUserNavigation } from '@/store/features/userSlice';
import { domainUrlPrefix } from '@/store/backEndCalls';
import { Tooltip } from '@mui/material';
import ClickAwayListener from '@mui/material/ClickAwayListener';
import { translateUsersOnline } from '../common/common';
import { useAuth } from 'react-oidc-context';

function ResponsiveAppBar() {
    const dispatch = useAppDispatch()
    const [anchorElNav, setAnchorElNav] = React.useState<null | HTMLElement>(null);
    const resultsPageEnabled = useAppSelector(state => state.viewStateSlice.appConfiguration.displayResultsBar);
    const usersOnline = useAppSelector(state => state.viewStateSlice.appInfo.usersOnline)
    const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);
    const auth = useAuth();

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

    const navigationOptions = Object.values(NavigationTab).filter(value => value !== NavigationTab.VoteStandings || resultsPageEnabled)
    return (
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
                    {/* User Button */}
                    <Box sx={{ flexGrow: 0 }}>
                        {auth && (
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
                                        <MenuItem onClick={() => auth.signoutRedirect({ post_logout_redirect_uri: window.location.origin })}>Logout</MenuItem>
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
    );
}
export default ResponsiveAppBar;