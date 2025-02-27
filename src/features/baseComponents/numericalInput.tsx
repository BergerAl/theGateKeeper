import React, { useState, useEffect } from 'react';
import OutlinedInput from '@mui/material/OutlinedInput';
import InputAdornment from '@mui/material/InputAdornment';
import styles from './baseComponents.module.css';
import { selectAreParametersReadonly } from '../controlPanel/controlPanelSlice';
import { useAppSelector } from '../../app/hooks';
import { Typography } from '@mui/material';

export const NumericalInput: React.FC<{ id: string, text: string, unit: string, value: number, setValue: any, minValue?: number, maxValue?: number }> =
    ({ id, text, unit, value, setValue, minValue, maxValue }) => {
        const disabled = useAppSelector(selectAreParametersReadonly)
        const decimalPlace = 4
        const regex = new RegExp(`^\\d+(\\.\\d{0,${decimalPlace}})?$`)
        const [errorColor, setErrorColor] = useState<React.CSSProperties>({ background: 'none' })

        const handleChange = (event: any) => {
            if (regex.test(event)) {
                setValue(event);
                if (maxValue && (event?.target?.value as number) > maxValue) {
                    setErrorColor({ background: '#FFCCCB' })
                }
                if (minValue && (event?.target?.value as number) < minValue) {
                    setErrorColor({ background: '#FFCCCB' })
                }
            }
        }

        const handleBlur = (event: any) => {
            setErrorColor({ background: 'none' })
            if (event?.target?.value === "") {
                setValue(0)
            }
            if (maxValue && (event?.target?.value as number) > maxValue) {
                setErrorColor({ background: '#FFCCCB' })
                setValue(maxValue)
            }
            if (minValue && (event?.target?.value as number) < minValue) {
                setErrorColor({ background: '#FFCCCB' })
                setValue(minValue)
            }
        }

        // Causes the field to turn red, if it is changed externally
        useEffect(() => {
            if (maxValue === value) {
                setErrorColor({ background: '#FFCCCB' })
            }
        }, [value, maxValue])

        const handleFocus = (e: any) => {
            e.target.select();
        };

        return (
            <div className={styles.numericalInput}>
                <Typography component="span" color="primary">{text}</Typography>
                <OutlinedInput
                    className={styles.numericalInputBox}
                    id={id}
                    endAdornment={<InputAdornment position="end">{unit}</InputAdornment>}
                    aria-describedby="outlined-weight-helper-text"
                    inputProps={{
                        'aria-label': 'weight',
                    }}
                    disabled={disabled}
                    value={value}
                    onChange={e => handleChange(e.target.value)}
                    onFocus={handleFocus}
                    onBlur={handleBlur}
                    style={errorColor}
                />
            </div>
        )
    };