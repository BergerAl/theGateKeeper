import { configureStore, ThunkAction, Action } from '@reduxjs/toolkit';
import viewStateSlice from '../features/baseComponents/baseComponentsSlice';
import controlPanelSlice from '../features/controlPanel/controlPanelSlice'
export const store = configureStore({
  reducer: {
    viewStateSlice: viewStateSlice,
    controlPanelSlice: controlPanelSlice
  },
});

export type AppDispatch = typeof store.dispatch;
export type RootState = ReturnType<typeof store.getState>;
export type AppThunk<ReturnType = void> = ThunkAction<
  ReturnType,
  RootState,
  unknown,
  Action<string>
>;
