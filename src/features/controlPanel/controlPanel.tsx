import { useAppSelector } from './../../app/hooks';
import React from 'react';
import { selectAreParametersReadonly, selectCurrentThemeMode, setAreParametersReadonly, setThemeMode } from './controlPanelSlice';
import { useDispatch } from 'react-redux';
import { Switch, Select, MenuItem, SelectChangeEvent } from '@mui/material';
import { themeStyle, themeStylesArray } from '../../context/themes';

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

        <div style={{ display: 'flex', flexDirection: 'column', position: 'fixed', bottom: '0px' }}>
            <strong>{"ControlPanel only for presentation purposes"}</strong>

            <div>
                <span >{"Parameters are readonly"}</span>
                <Switch checked={checked} onChange={handleChange} />
                <span >{"Activate Dark mode"}</span>
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
        </div>
    )
};