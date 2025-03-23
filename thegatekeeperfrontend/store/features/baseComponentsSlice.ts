import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { fetchAllUsers, healthCheck, voteForUser } from '../backEndCalls';

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

export interface Voting {
    isBlocked: boolean
    voteBlockedUntil: Date
}

export interface FrontEndInfo {
    name: string
    tier: string
    rank: string
    leaguePoints: number
    playedGames: number
    voting: Voting
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
        updateUsersIfBlocked:(state, action: PayloadAction<FrontEndInfo[]>) => {
            action.payload.forEach(element => {
                var playerIndex = state.frontEndInfo.findIndex(x => x.name == element.name)
                if (playerIndex !== -1) {
                    state.frontEndInfo[playerIndex].voting = element.voting
                  }
            });
        }
    },
    extraReducers(builder) {
        builder.addCase(fetchAllUsers.fulfilled, (state, action) => {
          state.frontEndInfo = action.payload;
        });
        builder.addCase(fetchAllUsers.rejected, (state, action) => {
            state.snackBarState = {text: action.error.message!, status: SnackBarStatus.Error};
        });
        builder.addCase(healthCheck.rejected, (state, action) => {
            state.snackBarState = {text: action.error.message!, status: SnackBarStatus.Error};
        });
        builder.addCase(voteForUser.fulfilled, (state, action) => {
            var playerIndex = state.frontEndInfo.findIndex(x => x.name == action.meta.arg)
            if (playerIndex !== -1) {
                state.frontEndInfo[playerIndex].voting.isBlocked = true
              }
            state.snackBarState = {text: `Voting was successful for ${action.meta.arg}`, status: SnackBarStatus.Ok};
        });
        builder.addCase(voteForUser.rejected, (state, action) => {
            state.snackBarState = {text: `Voting failed for ${action.meta.arg}`, status: SnackBarStatus.Error};
        });
      },
});

export const { 
    setSnackBarState, 
    setModalDialogState,
    updateUsersIfBlocked
} = viewStateSlice.actions;

export default viewStateSlice.reducer;
