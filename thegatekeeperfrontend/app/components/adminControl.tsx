import React from 'react';
import SwipeableDrawer from '@mui/material/SwipeableDrawer';
import { Select, MenuItem, InputLabel, FormControl, Box, SelectChangeEvent, Fab } from '@mui/material'
import AddIcon from '@mui/icons-material/Add'
import { useAppDispatch, useAppSelector } from '@/store/hooks';
import { DisplayedView } from '@/store/features/baseComponentsSlice';
import { updateConfiguration } from '@/store/backEndCalls';

interface SelectOption {
    value: string
    label: string
}

const AdminControl = () => {
    const dispatch = useAppDispatch()
    const [open, setOpen] = React.useState(false)
    const actualView = useAppSelector(state => state.viewStateSlice.appConfiguration)

    const options: SelectOption[] = (
        Object.keys(DisplayedView)
            .filter(key => isNaN(Number(key)))
            .map(key => ({
                value: DisplayedView[key as keyof typeof DisplayedView],
                label: key
            }))
    )

    const handleSelectChange = (event: SelectChangeEvent<string>) => {
        let clone = { ...actualView }
        clone.displayedView = event.target.value as any as DisplayedView
        dispatch(updateConfiguration(clone))
    }

    return (
        <>
            <Box sx={{ position: 'fixed', bottom: 16, right: 16, zIndex: 100 }}>
                <Fab
                    color="primary"
                    onClick={() => setOpen(true)}
                    sx={{ boxShadow: 3 }}
                >
                    <AddIcon />
                </Fab>
            </Box>
            <SwipeableDrawer
                anchor="right"
                open={open}
                onClose={() => setOpen(false)}
                onOpen={() => setOpen(true)}
                disableBackdropTransition={typeof window !== 'undefined' &&
                    /iPad|iPhone|iPod/.test(window.navigator.userAgent)}
                disableDiscovery={typeof window !== 'undefined' &&
                    /iPad|iPhone|iPod/.test(window.navigator.userAgent)}
            >
                <Box sx={{ width: 250, p: 2 }}>
                    <FormControl fullWidth>
                        <InputLabel>Select an option</InputLabel>
                        <Select
                            value={actualView.displayedView}
                            label="Select an option"
                            onChange={handleSelectChange}
                        >
                            {options.map((option) => (
                                <MenuItem key={option.value} value={option.value}>
                                    {option.label}
                                </MenuItem>
                            ))}
                        </Select>
                    </FormControl>
                </Box>
            </SwipeableDrawer>
        </>
    )
}

export default AdminControl
