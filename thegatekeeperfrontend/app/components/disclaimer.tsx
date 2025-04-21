import React from 'react';
import { Typography } from '@mui/material';

export const Disclaimer: React.FC = () => {
    return (
        <Typography component="span" color="primary" fontSize={12} style={{ margin: '0px 50px 0px 50px' }}> {"The Gate Keeper isn't endorsed by Riot Games and doesn't reflect the views or opinions of Riot Games or anyone officially involved in producing or managing Riot Games properties. Riot Games, and all associated properties are trademarks or registered trademarks of Riot Games, Inc."}</Typography>
    )
};