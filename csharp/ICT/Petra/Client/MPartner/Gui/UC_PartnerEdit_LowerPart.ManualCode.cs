//
// DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
//
// @Authors:
//       christiank
//
// Copyright 2004-2016 by OM International
//
// This file is part of OpenPetra.org.
//
// OpenPetra.org is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// OpenPetra.org is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with OpenPetra.org.  If not, see <http://www.gnu.org/licenses/>.
//
using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Ict.Common;
using Ict.Common.Data; // Implicit reference
using Ict.Common.Exceptions;
using Ict.Common.Verification;
using Ict.Petra.Client.App.Gui;
using Ict.Petra.Client.CommonForms;
using Ict.Petra.Client.MPartner.Logic;
using Ict.Petra.Shared.Interfaces.MPartner;
using Ict.Petra.Shared.MPartner;
using Ict.Petra.Shared.MPartner.Partner.Data;

namespace Ict.Petra.Client.MPartner.Gui
{
    public partial class TUC_PartnerEdit_LowerPart
    {
        #region Fields

        private TPartnerEditScreenLogic.TModuleTabGroupEnum FCurrentModuleTabGroup;
        private TPartnerEditTabPageEnum FInitiallySelectedTabPage;
        private List <string>FInitialisedChildUCs = new List <string>(3);

        /// <summary>holds a reference to the Proxy System.Object of the Serverside UIConnector</summary>
        private IPartnerUIConnectorsPartnerEdit FPartnerEditUIConnector;

        // private TDelegateIsNewPartner FDelegateIsNewPartner;


        #endregion

        #region Events

        /// <summary>todoComment</summary>
        public event THookupDataChangeEventHandler HookupDataChange;


        /// <summary>
        /// Raises Event HookupDataChange.
        /// </summary>
        /// <param name="e">Event parameters</param>
        /// <returns>void</returns>
        protected void OnHookupDataChange(System.EventArgs e)
        {
            if (HookupDataChange != null)
            {
                HookupDataChange(this, e);
            }
        }

        /// <summary>todoComment</summary>
        public event THookupPartnerEditDataChangeEventHandler HookupPartnerEditDataChange;

        /// <summary>
        /// Raises Event HookupPartnerEditDataChange.
        /// </summary>
        /// <param name="e">Event parameters</param>
        /// <returns>void</returns>
        protected void OnHookupPartnerEditDataChange(THookupPartnerEditDataChangeEventArgs e)
        {
            if (HookupPartnerEditDataChange != null)
            {
                HookupPartnerEditDataChange(this, e);
            }
        }

        /// <summary>todoComment</summary>
        public event TEnableDisableScreenPartsEventHandler EnableDisableOtherScreenParts;

        /// <summary>
        /// Raises Event EnableDisableOtherScreenParts.
        /// </summary>
        /// <param name="e">Event parameters</param>
        /// <returns>void</returns>
        protected void OnEnableDisableOtherScreenParts(TEnableDisableEventArgs e)
        {
            if (EnableDisableOtherScreenParts != null)
            {
                EnableDisableOtherScreenParts(this, e);
            }
        }

        /// <summary>todoComment</summary>
        public event TShowTabEventHandler ShowTab;

        /// <summary>
        /// Raises Event ShowTab.
        /// </summary>
        /// <param name="e">Event parameters</param>
        /// <returns>void</returns>
        protected void OnShowTab(TShowTabEventArgs e)
        {
            if (ShowTab != null)
            {
                ShowTab(this, e);
            }
        }

        /// <summary>
        /// This Event gets raised when a new Address got added and the Partner has got a Partner Status
        /// other than 'ACTIVE'.
        /// </summary>
        public event EventHandler AddressAddedPartnerNeedsToBecomeActive;

        /// <summary>
        /// Raises Event AddressAddedPartnerNeedsToBecomeActive.
        /// </summary>
        /// <param name="sender">Sender of the Event.</param>
        /// <param name="e">Event parameters.</param>
        protected void OnAddressAddedPartnerNeedsToBecomeActive(object sender, EventArgs e)
        {
            if (AddressAddedPartnerNeedsToBecomeActive != null)
            {
                AddressAddedPartnerNeedsToBecomeActive(sender, e);
            }
        }

        private void RunOnceOnParentActivationManual()
        {
            ucoPartnerTabSet.RunOnceOnParentActivation();
        }

        #endregion

        #region Properties

        /// <summary>todoComment</summary>
        public TPartnerEditScreenLogic.TModuleTabGroupEnum CurrentModuleTabGroup
        {
            get
            {
                return FCurrentModuleTabGroup;
            }

            set
            {
                FCurrentModuleTabGroup = value;
            }
        }

        /// <summary>todoComment</summary>
        public TPartnerEditTabPageEnum InitiallySelectedTabPage
        {
            get
            {
                return FInitiallySelectedTabPage;
            }

            set
            {
                FInitiallySelectedTabPage = value;
            }
        }

        /// <summary>Currently active TabPage (in any of the TabGroups). Read-only.</summary>
        public TPartnerEditTabPageEnum CurrentlySelectedTabPage
        {
            get
            {
                switch (FCurrentModuleTabGroup)
                {
                    case TPartnerEditScreenLogic.TModuleTabGroupEnum.mtgPartner:
                        return ucoPartnerTabSet.CurrentlySelectedTabPage;

                    case TPartnerEditScreenLogic.TModuleTabGroupEnum.mtgPersonnel:
                        return ucoPersonnelTabSet.CurrentlySelectedTabPage;

                    default:
                        // Fallback
                        return TPartnerEditTabPageEnum.petpAddresses;
                }
            }
        }

        /// <summary>used for passing through the Clientside Proxy for the UIConnector</summary>
        public IPartnerUIConnectorsPartnerEdit PartnerEditUIConnector
        {
            get
            {
                return FPartnerEditUIConnector;
            }

            set
            {
                FPartnerEditUIConnector = value;
            }
        }

        /// <summary>
        /// Returns the PartnerEditTDSPPartnerLocationRow DataRow of the currently selected Address.
        /// </summary>
        /// <remarks>Performs all necessary initialisations in case the Partner TabGroup and/or
        /// the Address Tab haven't been initialised before.</remarks>
        public PartnerEditTDSPPartnerLocationRow PartnerLocationDataRowOfCurrentlySelectedAddress
        {
            get
            {
                if (!ucoPartnerTabSet.IsDynamicallyLoadableTabSetUp(TUC_PartnerEdit_PartnerTabSet.TDynamicLoadableUserControls.dlucAddresses))
                {
                    InitChildUserControl(TPartnerEditScreenLogic.TModuleTabGroupEnum.mtgPartner);

                    // The follwing function calls internally 'DynamicLoadUserControl(TDynamicLoadableUserControls.dlucAddresses);'
                    ucoPartnerTabSet.SetUpPartnerAddressTab();
                }

                return ucoPartnerTabSet.LocationDataRowOfCurrentlySelectedAddress;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initialises the UserControl that has the Tabs for the currently selected Tab.
        /// </summary>
        private void InitChildUserControl(TPartnerEditScreenLogic.TModuleTabGroupEnum AModuleTabGroup)
        {
            switch (AModuleTabGroup)
            {
                case TPartnerEditScreenLogic.TModuleTabGroupEnum.mtgPartner:

                    if (!FInitialisedChildUCs.Contains(ucoPartnerTabSet.GetType().Name))
                    {
                        FInitialisedChildUCs.Add(ucoPartnerTabSet.GetType().Name);

                        this.ParentForm.Cursor = Cursors.WaitCursor;

                        ucoPartnerTabSet.PetraUtilsObject = FPetraUtilsObject;
                        ucoPartnerTabSet.PartnerEditUIConnector = FPartnerEditUIConnector;

                        if (!FInitialisedChildUCs.Contains(ucoPersonnelTabSet.GetType().Name))
                        {
                            ucoPartnerTabSet.InitiallySelectedTabPage = FInitiallySelectedTabPage;
                        }
                        else
                        {
                            ucoPartnerTabSet.InitiallySelectedTabPage = TPartnerEditTabPageEnum.petpAddresses;
                        }

                        ucoPartnerTabSet.MainDS = FMainDS;
                        ucoPartnerTabSet.SpecialInitUserControl();
                        ucoPartnerTabSet.HookupDataChange += new THookupDataChangeEventHandler(ucoPartnerTabSet_HookupDataChange);
                        ucoPartnerTabSet.HookupPartnerEditDataChange += new THookupPartnerEditDataChangeEventHandler(
                            ucoPartnerTabSet_HookupPartnerEditDataChange);
                        ucoPartnerTabSet.AddressAddedPartnerNeedsToBecomeActive += UcoPartnerTabSet_AddressAddedPartnerNeedsToBecomeActive;
                        this.ParentForm.Cursor = Cursors.Default;
                    }

                    break;

                case TPartnerEditScreenLogic.TModuleTabGroupEnum.mtgPersonnel:

                    if (!FInitialisedChildUCs.Contains(ucoPersonnelTabSet.GetType().Name))
                    {
                        FInitialisedChildUCs.Add(ucoPersonnelTabSet.GetType().Name);

                        this.ParentForm.Cursor = Cursors.WaitCursor;

                        ucoPersonnelTabSet.PetraUtilsObject = FPetraUtilsObject;
                        ucoPersonnelTabSet.PartnerEditUIConnector = FPartnerEditUIConnector;

                        if (!FInitialisedChildUCs.Contains(ucoPartnerTabSet.GetType().Name))
                        {
                            ucoPersonnelTabSet.InitiallySelectedTabPage = FInitiallySelectedTabPage;
                        }
                        else
                        {
                            ucoPersonnelTabSet.InitiallySelectedTabPage = TPartnerEditTabPageEnum.petpPersonnelIndividualData;
                        }

                        ucoPersonnelTabSet.MainDS = FMainDS;
                        ucoPersonnelTabSet.SpecialInitUserControl();

                        this.ParentForm.Cursor = Cursors.Default;
                    }

                    break;
            }

            FCurrentModuleTabGroup = AModuleTabGroup;
        }

        /// <summary>
        /// Shows the UserControl that has the Tabs for the currently selected Tab. If needed, initialisation
        /// of the UserContol is done.
        /// </summary>
        public void ShowChildUserControl(TPartnerEditScreenLogic.TModuleTabGroupEnum AModuleTabGroup)
        {
            InitChildUserControl(AModuleTabGroup);

            switch (AModuleTabGroup)
            {
                case TPartnerEditScreenLogic.TModuleTabGroupEnum.mtgPartner:

                    ucoPartnerTabSet.Visible = true;
                    ucoPersonnelTabSet.Visible = false;
                    ucoPartnerTabSet.SelectTabPage(ucoPartnerTabSet.CurrentlySelectedTabPage); // make refresh happen

                    break;

                case TPartnerEditScreenLogic.TModuleTabGroupEnum.mtgPersonnel:

                    ucoPersonnelTabSet.Visible = true;
                    ucoPartnerTabSet.Visible = false;
                    ucoPersonnelTabSet.SelectTabPage(ucoPersonnelTabSet.CurrentlySelectedTabPage); // make refresh happen

                    break;
            }
        }

        /// <summary>
        /// Switches to the corresponding TabPage.
        /// </summary>
        /// <remarks>If the TabPage is on a different TabGroup than the one that is currently
        /// shown, the TabGroup is first switched to (and it is initialised, if needed).</remarks>
        /// <param name="ATabPage">TapPage to switch to.</param>
        public void SelectTabPage(TPartnerEditTabPageEnum ATabPage)
        {
            TPartnerEditScreenLogic.TModuleTabGroupEnum ModuleTabGroup = TPartnerEditScreenLogic.DetermineTabGroup(ATabPage);

            ShowChildUserControl(ModuleTabGroup);

            switch (ModuleTabGroup)
            {
                case TPartnerEditScreenLogic.TModuleTabGroupEnum.mtgPartner:
                    ucoPartnerTabSet.SelectTabPage(ATabPage);
                    break;

                case TPartnerEditScreenLogic.TModuleTabGroupEnum.mtgPersonnel:
                    ucoPersonnelTabSet.SelectTabPage(ATabPage);
                    break;
            }
        }

        void ucoPartnerTabSet_HookupPartnerEditDataChange(object Sender, THookupPartnerEditDataChangeEventArgs e)
        {
            OnHookupPartnerEditDataChange(e);
        }

        void ucoPartnerTabSet_HookupDataChange(object Sender, EventArgs e)
        {
            OnHookupDataChange(e);
        }

        private void UcoPartnerTabSet_AddressAddedPartnerNeedsToBecomeActive(object sender, EventArgs e)
        {
            OnAddressAddedPartnerNeedsToBecomeActive(sender, e);
        }

        /// <summary>
        /// todoComment
        /// </summary>
        /// <param name="ADelegateFunction"></param>
        public void InitialiseDelegateIsNewPartner(TDelegateIsNewPartner ADelegateFunction)
        {
            // set the delegate function from the calling System.Object
            ucoPartnerTabSet.InitialiseDelegateIsNewPartner(ADelegateFunction);
        }

        /// <summary>
        /// Calls the DataSavedEventFired on certain nested UserControls.
        /// </summary>
        /// <param name="ASuccess">True if saving of data went ahead OK, otherwise false.</param>
        public void DataSavedEventFired(bool ASuccess)
        {
            ucoPartnerTabSet.DataSavedEventFired(ASuccess);
        }

        /// <summary>
        /// Performs data validation.
        /// </summary>
        /// <remarks>May be called by the Form that hosts this UserControl to invoke the data validation of
        /// the UserControl.</remarks>
        /// <param name="ADataValidationProcessingMode">Set to TErrorProcessingMode.Epm_All if data validation errors should be shown to the
        /// user, otherwise set it to TErrorProcessingMode.Epm_None.</param>
        /// <returns>True if data validation succeeded or if there is no current row, otherwise false.</returns>
        public bool ValidateAllData(TErrorProcessingMode ADataValidationProcessingMode)
        {
            bool ReturnValue = true;

            ReturnValue = ucoPartnerTabSet.ValidateAllData(ADataValidationProcessingMode);

            if (!ucoPersonnelTabSet.ValidateAllData(ADataValidationProcessingMode))
            {
                ReturnValue = false;
            }

            return ReturnValue;
        }

        /// <summary>
        /// Performs data validation for the currently displayed module tab group
        /// </summary>
        /// <returns>True if data validation succeeded or if there is no current row, otherwise false.</returns>
        public bool ValidateCurrentModuleTabGroupData()
        {
            bool ReturnValue = true;

            FPetraUtilsObject.VerificationResultCollection.Clear();

            switch (CurrentModuleTabGroup)
            {
                case TPartnerEditScreenLogic.TModuleTabGroupEnum.mtgPartner:
                    ucoPartnerTabSet.ValidateAllData(TErrorProcessingMode.Epm_None);
                    break;

                case TPartnerEditScreenLogic.TModuleTabGroupEnum.mtgPersonnel:
                    ucoPersonnelTabSet.ValidateAllData(TErrorProcessingMode.Epm_None);
                    break;

                default:
                    break;
            }

            ReturnValue = TDataValidation.ProcessAnyDataValidationErrors(false, FPetraUtilsObject.VerificationResultCollection,
                this.GetType(), null, true);

            if (ReturnValue)
            {
                // Remove a possibly shown Validation ToolTip as the data validation succeeded
                FPetraUtilsObject.ValidationToolTip.RemoveAll();
            }

            return ReturnValue;
        }

        /// <summary>
        /// Gets the data from all controls on this TabControl.
        /// The data is stored in the DataTables/DataColumns to which the Controls
        /// are mapped.
        /// </summary>
        public void GetDataFromControls()
        {
            ucoPartnerTabSet.GetDataFromControls();
            ucoPersonnelTabSet.GetDataFromControls();
        }

        /// <summary>
        /// Returns true if a new location is being added, otherwise false.
        /// </summary>
        /// <returns>True if a new location is being added, otherwise false.</returns>
        public bool LocationBeingAdded()
        {
            if (FCurrentModuleTabGroup == TPartnerEditScreenLogic.TModuleTabGroupEnum.mtgPartner)
            {
                return ucoPartnerTabSet.LocationBeingAdded();
            }
            else
            {
                throw new EOPAppException("This Menu item must only be used when the Partner Data Tab Group is shown");
            }
        }

        /// <summary>
        /// Checks whether the Partner has an Address with the specified PK.
        /// </summary>
        /// <remarks>Displays an error message to the user if it will return false!</remarks>
        /// <param name="ALocationPK">PK of a DataRow in the PLocation table.</param>
        /// <returns>True if the Address was found, otherwise false.</returns>
        public bool IsAddressRowPresent(TLocationPK ALocationPK)
        {
            if (FCurrentModuleTabGroup == TPartnerEditScreenLogic.TModuleTabGroupEnum.mtgPartner)
            {
                return ucoPartnerTabSet.IsAddressRowPresent(ALocationPK);
            }
            else
            {
                throw new EOPAppException("This Menu item must only be used when the Partner Data Tab Group is shown");
            }
        }

        /// <summary>
        /// Allows copying of an Address that the user has found (using Menu 'Edit'->'Find
        /// New Address...') into the currently edited Address.
        /// </summary>
        /// <param name="AFoundAddressLocationRow">DataRow containing the Location information
        /// for the found Address.
        /// </param>
        public void CopyFoundAddressData(PLocationRow AFoundAddressLocationRow)
        {
            if (FCurrentModuleTabGroup == TPartnerEditScreenLogic.TModuleTabGroupEnum.mtgPartner)
            {
                ucoPartnerTabSet.CopyFoundAddressData(AFoundAddressLocationRow);
            }
            else
            {
                throw new EOPAppException("This Menu item must only be used when the Partner Data Tab Group is shown");
            }
        }

        /// <summary>
        /// Allows adding an Address that the user has found (using Menu 'Edit'->'Find
        /// New Address...').
        /// </summary>
        /// <remarks>The found Address must be merged into the PLocation DataTable in the
        /// FMultiTableDS before this function can be called! The record that was merged
        /// gets deleted by a call to this procedure!</remarks>
        /// <param name="ASiteKey">SiteKey of the found Location.</param>
        /// <param name="ALocationKey">LocationKey of the found Location.</param>
        public void AddNewFoundAddress(Int64 ASiteKey, Int32 ALocationKey)
        {
            if (FCurrentModuleTabGroup == TPartnerEditScreenLogic.TModuleTabGroupEnum.mtgPartner)
            {
                ucoPartnerTabSet.AddNewFoundAddress(ASiteKey, ALocationKey);
            }
            else
            {
                throw new EOPAppException("This Menu item must only be used when the Partner Data Tab Group is shown");
            }
        }

        /// <summary>
        /// todoComment
        /// </summary>
        public void CleanupRecordsBeforeMerge()
        {
            if (FCurrentModuleTabGroup == TPartnerEditScreenLogic.TModuleTabGroupEnum.mtgPartner)
            {
                ucoPartnerTabSet.CleanupRecordsBeforeMerge();
            }
        }

        /// <summary>
        /// todoComment
        /// </summary>
        public void RefreshRecordsAfterMerge()
        {
            if (FCurrentModuleTabGroup == TPartnerEditScreenLogic.TModuleTabGroupEnum.mtgPartner)
            {
                ucoPartnerTabSet.RefreshRecordsAfterMerge();
            }
        }

        /// <summary>
        /// todoComment
        /// </summary>
        /// <param name="AParameterDT"></param>
        public void SimilarLocationsProcessing(PartnerAddressAggregateTDSSimilarLocationParametersTable AParameterDT)
        {
            if (FCurrentModuleTabGroup == TPartnerEditScreenLogic.TModuleTabGroupEnum.mtgPartner)
            {
                if (!ucoPartnerTabSet.IsDynamicallyLoadableTabSetUp(TUC_PartnerEdit_PartnerTabSet.TDynamicLoadableUserControls.dlucAddresses))
                {
                    // The follwing function calls internally 'DynamicLoadUserControl(TDynamicLoadableUserControls.dlucAddresses);'
                    ucoPartnerTabSet.SetUpPartnerAddressTab();
                }

                ucoPartnerTabSet.ProcessServerResponseSimilarLocations(AParameterDT);
            }
        }

        /// <summary>
        /// todoComment
        /// </summary>
        /// <param name="AAddedOrChangedPromotionDT"></param>
        /// <param name="AParameterDT"></param>
        public void AddressAddedOrChangedProcessing(PartnerAddressAggregateTDSAddressAddedOrChangedPromotionTable AAddedOrChangedPromotionDT,
            PartnerAddressAggregateTDSChangePromotionParametersTable AParameterDT)
        {
            if (FCurrentModuleTabGroup == TPartnerEditScreenLogic.TModuleTabGroupEnum.mtgPartner)
            {
                if (!ucoPartnerTabSet.IsDynamicallyLoadableTabSetUp(TUC_PartnerEdit_PartnerTabSet.TDynamicLoadableUserControls.dlucAddresses))
                {
                    // The follwing function calls internally 'DynamicLoadUserControl(TDynamicLoadableUserControls.dlucAddresses);'
                    ucoPartnerTabSet.SetUpPartnerAddressTab();
                }

                ucoPartnerTabSet.ProcessServerResponseAddressAddedOrChanged(AAddedOrChangedPromotionDT, AParameterDT);
            }
        }

        /// <summary>
        /// todoComment
        /// </summary>
        public void RefreshPersonnelDataAfterMerge(bool APartnerAttributesOrRelationsChanged)
        {
            ucoPersonnelTabSet.RefreshPersonnelDataAfterMerge(APartnerAttributesOrRelationsChanged);
        }

        /// <summary>
        /// Refreshes the Family Members list on the Family tab
        /// </summary>
        public void RefreshFamilyMembersList(TFormsMessage AFormsMessage)
        {
            ucoPartnerTabSet.RefreshFamilyMembersList(AFormsMessage);
        }

        /// <summary>
        /// Refreshes position in Uni Hierarchy
        /// </summary>
        public void RefreshUnitHierarchy(Tuple <string, Int64, Int64>AUnitHierarchyChange)
        {
            ucoPartnerTabSet.RefreshUnitHierarchy(AUnitHierarchyChange);
        }

        /// <summary>
        /// Selects the given contact log.
        /// </summary>
        /// <param name="AContactLogID">Contact Log identifier.</param>
        public void SelectContactLog(string AContactLogID)
        {
            ucoPartnerTabSet.SelectContactLog(AContactLogID);
        }

        /// <summary>
        /// Selects the given application in the Personnel Applications tab.
        /// </summary>
        /// <param name="AApplicationKey">Application's application key.</param>
        /// <param name="ARegistrationOffice">Application's registration office.</param>
        public void SelectApplication(Int32 AApplicationKey, Int64 ARegistrationOffice)
        {
            ucoPersonnelTabSet.SelectApplication(AApplicationKey, ARegistrationOffice);
        }

        #endregion

        #region Menu and command key handlers for our user controls

        ///////////////////////////////////////////////////////////////////////////////
        //// Special Handlers for menus and command keys for our user controls

        /// <summary>
        /// Handler for command key processing
        /// </summary>
        private bool ProcessCmdKeyManual(ref Message msg, Keys keyData)
        {
            if ((FCurrentModuleTabGroup == TPartnerEditScreenLogic.TModuleTabGroupEnum.mtgPartner)
                && this.ucoPartnerTabSet.ProcessParentCmdKey(ref msg, keyData))
            {
                return true;
            }

            if ((FCurrentModuleTabGroup == TPartnerEditScreenLogic.TModuleTabGroupEnum.mtgPersonnel)
                && this.ucoPersonnelTabSet.ProcessParentCmdKey(ref msg, keyData))
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}