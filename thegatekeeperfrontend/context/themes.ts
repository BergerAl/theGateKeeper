import {
    createTheme,
} from '@mui/material';

export type themeStyle = 'light' | 'dark' | 'vibrant'

export const themeStylesArray = ['light', 'dark', 'vibrant']

export const lightTheme = createTheme({
    palette: {
        mode: 'light',
        primary: {
            main: '#1976d2',
        },
        secondary: {
            main: '#dc004e',
        },
        background: {
            default: '#ffffff',
            paper: '#f5f5f5'
        }
    }
});

export const darkTheme = createTheme({
    palette: {
        mode: 'dark',
        primary: {
            main: '#90caf9',
        },
        secondary: {
            main: '#ff80ab',
        },
        background: {
            default: '#121212',
            paper: '#1e1e1e'
        }
    }
});

export const vibrantTheme = createTheme({
    palette: {
        mode: 'light',
        primary: {
            main: '#ff4081',
        },
        secondary: {
            main: '#303f9f',
        },
        background: {
            default: '#fff3f0',
            paper: '#ffebee'
        }
    }
});