import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { RootState } from '../store';
import { themeStyle } from '../../context/themes';

export enum NavigationTab {
  LeagueStandings = "LeagueStandings",
  VoteStandings = "VoteStandings"
}

export interface UserSlice {
  selectedTheme: themeStyle
  currentNavigation: NavigationTab
}

const initialState: UserSlice = {
  selectedTheme: 'dark',
  currentNavigation: NavigationTab.LeagueStandings
};

export const userSlice = createSlice({
  name: 'controlPanelState',
  initialState,
  reducers: {
    setThemeMode: (state, action: PayloadAction<themeStyle>) => {
      state.selectedTheme = action.payload;
    },
    setUserNavigation: (state, action: PayloadAction<NavigationTab>) => {
      state.currentNavigation = action.payload;
    }
  },
});

export const {
  setThemeMode,
  setUserNavigation
} = userSlice.actions;

export const selectCurrentThemeMode = (state: RootState) => state.userSlice.selectedTheme

export default userSlice.reducer;
