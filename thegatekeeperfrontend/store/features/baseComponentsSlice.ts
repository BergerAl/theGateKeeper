import { createSlice, PayloadAction, createAsyncThunk } from '@reduxjs/toolkit';

export enum SnackBarStatus {
    Ok,
    Warning,
    Error
}

export enum ModalTypeDialog {
    None,
    WorkpieceCreation,
    DeleteWorkpiece
}

export interface SnackBarState {
    text: string,
    status?: SnackBarStatus
}

export interface FrontEndInfo {
    name: string
    tier: string
    rank: string
    leaguePoints: number
    playedGames: number
}

export interface ViewState {
    actualSelectedPage: number
    snackBarState: SnackBarState
    modalDialog: {
        modalDialogType: ModalTypeDialog
        visible: boolean
    },
    frontEndInfo: FrontEndInfo[]
}

export const initialState: ViewState = {
    actualSelectedPage: 0,
    snackBarState: { text: "", status: SnackBarStatus.Ok },
    modalDialog: {
        modalDialogType: ModalTypeDialog.None,
        visible: false
    },
    frontEndInfo: []
};

export const viewStateSlice = createSlice({
    name: 'viewState',
    initialState,
    reducers: {
        setSnackBarState: (state, action: PayloadAction<SnackBarState>) => {
            state.snackBarState = action.payload;
        },
        setModalDialogState: (state, action: PayloadAction<{ visible: boolean, modalDialogType?: ModalTypeDialog }>) => {
            state.modalDialog = {visible: action.payload.visible, modalDialogType: action.payload?.modalDialogType ? action.payload?.modalDialogType : ModalTypeDialog.None };
        },
    },
    extraReducers(builder) {
        builder.addCase(fetchAllUsers.pending, (state) => {
        //   state.allAvailableCyclesRequest.status = 'loading';
        });
        builder.addCase(fetchAllUsers.fulfilled, (state, action) => {
        //   state.allAvailableCyclesRequest.status = 'success';
          state.frontEndInfo = action.payload;
        });
        builder.addCase(fetchAllUsers.rejected, (state, action) => {
        //   state.allAvailableCyclesRequest.status = 'failed';
        });
      },
});

export const fetchAllUsers = createAsyncThunk(
    'productionSheet/allAvailableCycles',
    async () => {
      const response = await fetch('/api/TheGateKeeper/getCurrentRanks');
      if (!response.ok) {
        throw new Error(`Failed to fetch all available cycles: ${response.statusText}`);
      }
  
      const data = await response.json();
      return data;
    }
  );

export const { setSnackBarState, setModalDialogState } = viewStateSlice.actions;

export default viewStateSlice.reducer;
