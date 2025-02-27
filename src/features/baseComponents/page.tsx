import React, { useEffect } from 'react';
import { useAppSelector, useAppDispatch } from '../../app/hooks';
import { selectActualSelectedPage, setActualSelectedPage } from './baseComponentsSlice';
import { TabsComponent } from './tabsComponent';

export const PageComponent: React.FC = () =>  {
    const currentPage = useAppSelector(selectActualSelectedPage)
    const dispatch = useAppDispatch();

    const handleChange = (event: React.SyntheticEvent, newValue: number) => {
        dispatch(setActualSelectedPage(newValue));
      };

    useEffect(() => {
        return () => {
            dispatch(setActualSelectedPage(0))
        };
    // eslint-disable-next-line
    }, []);

    return (
        <TabsComponent value={currentPage} setValue={handleChange} selection={["Geometrie", "Technologie"]} /> 
    )
}