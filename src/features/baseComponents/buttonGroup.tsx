import * as React from 'react';
import Button from '@mui/material/Button';
import ButtonGroup from '@mui/material/ButtonGroup';

export type textFunctionMap = {
    displayText: string;
    function: () => void;
};

export const BasicButtonGroup: React.FC<{ buttonMap: textFunctionMap[] }> = ({ buttonMap }) => {
    return (
        <ButtonGroup variant="contained" aria-label="Basic button group">
            {buttonMap.map((button) => { return <Button onPointerDown={button.function} key={button.displayText}>{button.displayText}</Button> })}
        </ButtonGroup>
    );
}