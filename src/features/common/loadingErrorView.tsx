import React from 'react';
import { BackendRequest } from '../common/types';
import CircularProgress from '@mui/material/CircularProgress';
import Box from '@mui/material/Box';

export const LoadingErrorView: React.FC<{ backendRequest: BackendRequest, textOnError: string }> = ({ backendRequest, textOnError }) => {
    if (backendRequest.status === 'loading') {
        return (
            <Box sx={{ display: 'flex' }}>
                <CircularProgress />
            </Box>
        )
    }
    if (backendRequest.status === 'failed') {
        return (
            <>
                <div style={{ display: 'flex', flexDirection: 'column' }}>
                    {textOnError}
                    <button onPointerDown={() => window.location.reload()}>
                        Refresh
                    </button>
                </div>

            </>
        )
    }
    return <></>
};