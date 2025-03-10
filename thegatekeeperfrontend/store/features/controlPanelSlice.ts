import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { RootState } from '../store';
import { themeStyle } from '../../context/themes';

export interface ControlPanelState {
  areParametersReadonly: boolean,
  selectedTheme: themeStyle
}

const initialState: ControlPanelState = {
  areParametersReadonly: false,
  selectedTheme: 'dark'
};

export const controlPanelSlice = createSlice({
  name: 'controlPanelState',
  initialState,
  reducers: {
    setAreParametersReadonly: (state, action: PayloadAction<boolean>) => {
      state.areParametersReadonly = action.payload;
    },
    setThemeMode: (state, action: PayloadAction<themeStyle>) => {
      state.selectedTheme = action.payload;
    },
  },
});

export const {
  setAreParametersReadonly,
  setThemeMode
} = controlPanelSlice.actions;

export const selectAreParametersReadonly = (state: RootState) => state.controlPanelSlice.areParametersReadonly
export const selectCurrentThemeMode = (state: RootState) => state.controlPanelSlice.selectedTheme

export default controlPanelSlice.reducer;
