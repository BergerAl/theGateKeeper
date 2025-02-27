import React from 'react';
import Switch from '@mui/material/Switch';
import { Typography } from '@mui/material';
import styles from './baseComponents.module.css';
import { useAppSelector } from '../../app/hooks';
import { selectAreParametersReadonly } from '../controlPanel/controlPanelSlice';

export const SwitchComponent: React.FC<{ text: string, checked: boolean, setValue: any }> = ({ text, checked, setValue }) => {


    const disabled = useAppSelector(selectAreParametersReadonly)
    return (
        <div className={styles.numericalInput}>
            <Typography component="span" color="primary">{text}</Typography>
            <Switch checked={checked} onChange={setValue} disabled={disabled} />
        </div>
    )
};