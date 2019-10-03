﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ISPRC.Models
{
    /* Admin; Manage Accounts, 
     * Club Owner; Create Race
     * Member; Register, Add Bird (supply band number)
    */

    public class JoinViewModel
    {
        public int RaceId { get; set; }
        public string RaceName { get; set; }
        [Required]
        public int BirdId { get; set; }
    }

    public class AdminAccountModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool Locked { get; set; }
        public DateTime? DateCreated { get; set; }
    }

    public class Race
    {
        [Key]
        public int RaceId { get; set; }
        [Display(Name ="Race Name")]
        public string RaceName { get; set; }
        [Display(Name = "Race Start Date")]
        public DateTime RaceStartDate { get; set; }
        [Display(Name = "Race Cut-off Date")]
        public DateTime RaceCutOffDate { get; set; }
        [Display(Name = "Race Ended Date")]
        public DateTime? RaceEndedDate { get; set; } // if prematurely ended by ForceRaceDone
        [Display(Name = "Date Created")]
        public DateTime DateCreated { get; set; }
        [Display(Name = "Race Latitude")]
        public string RaceLatitudeCoordinate { get; set; }
        [Display(Name = "Race Longitude")]
        public string RaceLongitudeCoordinate { get; set; }
        [Display(Name = "Forced Done")]
        public bool ForceRaceDone { get; set; } // prematurely end the race

        public bool RaceDone
        {
            get
            {
                if (ForceRaceDone == true)
                {
                    return true;
                }
                else
                {
                    return (DateTime.UtcNow.AddHours(8) > RaceStartDate) ? true : false;
                }
            }
        }

        public virtual List<BirdRace> Birds { get; set; }
    }

    public class Bird
    {
        [Key]
        public int BirdId { get; set; }
        [Display(Name = "Band Number")]
        public string BirdName { get; set; }
        [Display(Name = "Date Created")]
        public DateTime DateCreated { get; set; }

        public string OwnerId { get; set; }
        [ForeignKey("OwnerId")]
        public virtual ApplicationUser Owner { get; set; }

        public virtual List<BirdRace> Races { get; set; }
    }

    public class BirdRace
    {
        [Key]
        public int BirdRaceId { get; set; }

        public int BirdId { get; set; }
        [ForeignKey("BirdId")]
        public Bird Bird { get; set; }

        public int RaceId { get; set; }
        [ForeignKey("RaceId")]
        public Race Race { get; set; }

        [Display(Name = "End Longitude")]
        public string EndLatitude { get; set; }
        [Display(Name = "End Latitude")]
        public string EndLongitude { get; set; }

        [Display(Name = "Release Date")]
        public DateTime? ReleaseDate { get; set; }
        [Display(Name = "Arrival Date")]
        public DateTime? ArrivalDate { get; set; }

        public double? Speed { get; set; }
        public double? Distance { get; set; } // if End Latitude and End Latitude is not null, use Haversine formula
        // public double? Time { get; set; }

        [Display(Name = "Bird Code")]
        public string BirdCode { get; set; }

        public DateTime DateCreated { get; set; }
    }
}