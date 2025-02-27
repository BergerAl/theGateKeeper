import React from 'react';
import MenuItem from '@mui/material/MenuItem';
import { Typography } from '@mui/material';
import Select, { SelectChangeEvent } from '@mui/material/Select';
import styles from './baseComponents.module.css';
import { useAppSelector } from '../../app/hooks';
import { selectAreParametersReadonly } from '../controlPanel/controlPanelSlice';

export type keyPairValue = {
    displayText: string;
    value: string;
};

export const SelectInput: React.FC<{ text: string, selection: keyPairValue[], value: string, setValue: any }> = ({ text, selection, value, setValue }) => {
    const disabled = useAppSelector(selectAreParametersReadonly)
    const handleChange = (event: SelectChangeEvent) => {
        setValue(event.target.value as string);
    };

    return (
        <div className={styles.numericalInput}>
            <Typography component="span" color="primary">{text}</Typography>
            <Select
                className={styles.selectBox}
                labelId="selectInputLabel"
                id="selectInputId"
                disabled={disabled}
                value={value}
                onChange={handleChange}
                inputProps={{ 'aria-label': 'Without label' }}
            >
                {selection.map((entry) => { return <MenuItem key={entry.value} value={entry.value}>{entry.displayText}</MenuItem> })}
            </Select>
        </div>
    )
};