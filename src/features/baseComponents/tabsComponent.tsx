import React from 'react';
import Tabs from '@mui/material/Tabs';
import Tab from '@mui/material/Tab';

export const TabsComponent: React.FC<{ selection: string[], value: number, setValue: any }> = ({ selection, value, setValue }) =>  {
    return (
        <Tabs value={value} onChange={setValue} aria-label="icon tabs example">
            {selection.map((entry) => { return <Tab key={entry} label={entry} aria-label="phone" /> })}
        </Tabs>
    );
}