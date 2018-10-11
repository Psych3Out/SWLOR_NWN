﻿using System;
using System.Linq;
using NWN;
using SWLOR.Game.Server.Data.Contracts;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.Service.Contracts;
using SWLOR.Game.Server.ValueObject.Dialog;
using static NWN.NWScript;
using Object = NWN.Object;

namespace SWLOR.Game.Server.Conversation
{
    public class ApartmentEntrance : ConversationBase
    {
        private readonly IDataContext _db;
        private readonly IAreaService _area;
        private readonly IBaseService _base;

        public ApartmentEntrance(
            INWScript script,
            IDialogService dialog,
            IDataContext db,
            IAreaService area,
            IBaseService @base)
            : base(script, dialog)
        {
            _db = db;
            _area = area;
            _base = @base;
        }

        public override PlayerDialog SetUp(NWPlayer player)
        {
            PlayerDialog dialog = new PlayerDialog("MainPage");

            DialogPage mainPage = new DialogPage("Please select which apartment you would like to enter from the list below. If you do not have an apartment but would like to rent one please use the nearby Apartment Terminal.");

            dialog.AddPage("MainPage", mainPage);
            return dialog;
        }

        public override void Initialize()
        {
            LoadMainPage();
        }

        public override void DoAction(NWPlayer player, string pageName, int responseID)
        {
            MainPageResponses(responseID);
        }

        private void LoadMainPage()
        {
            NWPlaceable door = Object.OBJECT_SELF;
            int apartmentBuildingID = door.GetLocalInt("APARTMENT_BUILDING_ID");

            if (apartmentBuildingID <= 0)
            {
                _.SpeakString("APARTMENT_BUILDING_ID is not set. Please inform an admin.");
                return;
            }

            ClearPageResponses("MainPage");

            var player = GetPC();
            var apartments = _db.PCBases.Where(x => x.PlayerID == player.GlobalID &&
                                                         x.ApartmentBuildingID == apartmentBuildingID &&
                                                         x.DateRentDue > DateTime.UtcNow)
                                             .OrderBy(o => o.DateInitialPurchase);

            int count = 1;
            foreach (var apartment in apartments)
            {
                string name = "Apartment #" + count;

                if (!string.IsNullOrWhiteSpace(apartment.CustomName))
                {
                    name = apartment.CustomName;
                }

                AddResponseToPage("MainPage", name, true, apartment.PCBaseID);

                count++;
            }

        }

        private void MainPageResponses(int responseID)
        {
            var response = GetResponseByID("MainPage", responseID);
            int pcApartmentID = response.CustomData[string.Empty];
            EnterApartment(pcApartmentID);
        }

        private void EnterApartment(int pcBaseID)
        {
            NWPlayer oPC = GetPC();

            var apartment = _db.PCBases.Single(x => x.PCBaseID == pcBaseID);
            NWArea instance = GetAreaInstance(pcBaseID);

            if (instance == null)
            {
                string name = oPC.Name + "'s Apartment";
                if (!string.IsNullOrWhiteSpace(apartment.CustomName))
                {
                    name = apartment.CustomName;
                }

                instance = _area.CreateAreaInstance(apartment.BuildingStyle.Resref, name);
                instance.SetLocalInt("PC_BASE_ID", pcBaseID);
                instance.SetLocalInt("BUILDING_TYPE", (int)BuildingType.Apartment);

                foreach (var furniture in apartment.PCBaseStructures)
                {
                    _base.SpawnStructure(instance, furniture.PCBaseStructureID);
                }
            }

            _base.JumpPCToBuildingInterior(oPC, instance);
        }



        private NWArea GetAreaInstance(int pcApartmentID)
        {
            NWArea instance = null;
            foreach (var area in NWModule.Get().Areas)
            {
                if (area.GetLocalInt("PC_BASE_ID") == pcApartmentID)
                {
                    instance = area;
                    break;
                }
            }

            return instance;
        }

        public override void EndDialog()
        {
        }
    }
}
