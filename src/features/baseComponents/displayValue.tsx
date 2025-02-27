import React from 'react';
import Box from '@mui/material/Box';
import styles from './baseComponents.module.css';
import { Typography } from '@mui/material';

export const DisplayValue: React.FC<{ text: string, value: string | number, unit: string}> = ({ text, value, unit }) => {

    return (
        <div className={styles.numericalInput}>
            <Typography component="span" color="primary">{text}</Typography>
            <Typography component="span" color="primary" sx={{ display: 'inline' }}>{`${value} ${unit}`}</Typography>
        </div>
    )
};