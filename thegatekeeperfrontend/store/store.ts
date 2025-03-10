import { configureStore } from '@reduxjs/toolkit'
import viewStateSlice from './features/baseComponentsSlice'
import controlPanelSlice from './features/controlPanelSlice'

export const makeStore = () => {
  return configureStore({
    reducer: {
        viewStateSlice: viewStateSlice,
        controlPanelSlice: controlPanelSlice
    }
  })
}

// Infer the type of makeStore
export type AppStore = ReturnType<typeof makeStore>
// Infer the `RootState` and `AppDispatch` types from the store itself
export type RootState = ReturnType<AppStore['getState']>
export type AppDispatch = AppStore['dispatch']
