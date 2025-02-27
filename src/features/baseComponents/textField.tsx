import * as React from 'react';
import { TextField as MuiTextField } from "@mui/material";
import { useAppSelector } from "../../app/hooks";
import { selectAreParametersReadonly } from "../controlPanel/controlPanelSlice";

export const TextField: React.FC<{ text: string, rowsAmount: number, value: string, setValue: React.Dispatch<React.SetStateAction<string>>, handleBlur: any }> = ({ text, rowsAmount, value, setValue, handleBlur }) => {
    const disabled = useAppSelector(selectAreParametersReadonly)
    return (
        <MuiTextField
            id="outlined-multiline-static"
            label={text}
            fullWidth
            multiline={rowsAmount > 1 ? true : false}
            rows={rowsAmount}
            value={value}
            disabled={disabled}
            onChange={(event: React.ChangeEvent<HTMLInputElement>) => {
                setValue(event.target.value);
            }}
            onBlur={handleBlur}
            onPointerOut={handleBlur}
        />
    )
};