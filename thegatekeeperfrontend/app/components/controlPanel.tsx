import React from 'react';
import { useDispatch } from 'react-redux';
import { Select, MenuItem, SelectChangeEvent, Typography, Divider } from '@mui/material';
import { themeStyle, themeStylesArray } from '../../context/themes';
import { useAppSelector } from '@/store/hooks';
import { selectAreParametersReadonly, selectCurrentThemeMode, setAreParametersReadonly, setThemeMode } from '@/store/features/controlPanelSlice';
import { Disclaimer } from './disclaimer';

export const ControlPanel: React.FC = () => {
    const checked = useAppSelector(selectAreParametersReadonly)
    const theme = useAppSelector(selectCurrentThemeMode)
    const dispatch = useDispatch()

    const handleChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        dispatch(setAreParametersReadonly(event.target.checked));
    };

    const handleThemeChange = (event: SelectChangeEvent) => {
        dispatch(setThemeMode(event.target.value as themeStyle));
    };
    return (

        <div style={{ display: 'flex', flexDirection: 'column', position: 'fixed', bottom: '0%' }}>
            <div style={{marginBottom: '1%'}}>
                <Typography component="span" color="primary" style={{marginRight: '15px'}}>{"Select Theme"}</Typography>
                <Select
                    labelId="selectThemeLabel"
                    id="selectThemeId"
                    value={theme}
                    onChange={handleThemeChange}
                    inputProps={{ 'aria-label': 'Without label' }}
                >
                    {themeStylesArray.map((entry) => { return <MenuItem key={entry} value={entry}>{entry}</MenuItem> })}
                </Select>
            </div>
            <Divider />
            <Disclaimer />
        </div>
    )
};