import React, { useEffect, useState } from 'react';
import Snackbar from '@mui/material/Snackbar';
import IconButton from '@mui/material/IconButton';
import CloseIcon from '@mui/icons-material/Close';
import { useAppDispatch, useAppSelector } from '@/store/hooks';
import { setSnackBarState, SnackBarStatus } from '@/store/features/baseComponentsSlice';

export const SimpleSnackbar: React.FC = () => {
    const dispatch = useAppDispatch()
    const snackBarState = useAppSelector(state => state.viewStateSlice.snackBarState)
    const [backgroundColor, setBackGroundColor] = useState<string>('green')

    useEffect(() => {
        switch (snackBarState.status) {
            case SnackBarStatus.Error:
                setBackGroundColor('red')
                break;
            case SnackBarStatus.Warning:
                setBackGroundColor('yellow')
                break;
            case SnackBarStatus.Ok:
                setBackGroundColor('green')
                break;
            default:
                break;
        }
    }, [snackBarState.status, setBackGroundColor]);

    useEffect(() => {
        if (snackBarState.text !== "") {
            const timerId = setTimeout(() => {
                dispatch(setSnackBarState({ text: "" }))
            }, 4000);
            return () => {
                clearTimeout(timerId);
            };
        }
    }, [snackBarState.text, dispatch]);

    const action = (
        <React.Fragment>
            <IconButton
                size="small"
                aria-label="close"
                color="inherit"
                onClick={() => dispatch(setSnackBarState({ text: "" }))}
            >
                <CloseIcon fontSize="small" />
            </IconButton>
        </React.Fragment>
    );

    return (
        <div>
            <Snackbar
                anchorOrigin={{ vertical: 'top', horizontal: 'center' }}
                open={snackBarState.text !== ""}
                onClick={() => dispatch(setSnackBarState({ text: "" }))}
                message={snackBarState.text}
                action={action}
                ContentProps={{
                    sx: {
                        color: "black",
                        bgcolor: backgroundColor,
                        fontWeight: "bold",
                    }
                }}
            />
        </div>
    );
}