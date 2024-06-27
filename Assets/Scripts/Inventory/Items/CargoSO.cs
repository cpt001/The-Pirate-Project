using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "CargoItem")]
public class CargoSO : ScriptableObject
{
    public enum CargoType
    {
        _undefined,
        Wood,               //Producer: Logging Camp, Shack
            //Consumer: Apiary, Armorer, Bakery, Blacksmith, Butcher, Carpenter, Drydock, Fishing Hut
        Honey,              //Producer: Apiary, Hunter Shack
            //Consumer: Apothecary, Bakery, Market Stalls, Shacks, House
        Roots,              //Producer: Shack, Gypsy, Apothecary
            //Consumer: Bakery, Shacks, Gypsy, Apothecary, Bawdy House
        Animal_Parts,       //Producer: Gypsy, Hunter, Apothecary, Dock
            //Consumer: Gypsy, Hunter, Leathersmith, Apothecary, Lighthouse, Prison
        Animal_Fat,
        Water,              //Producer: Well
            //Consumer: Apothecary, Bakery, Barn, Bawdy House, Blacksmith, Butcher, Distillery, Forge, Gypsy, House, Jeweler, Mill, Prison, Shack, Tar Kiln
        Steel_Ingot,        //Producer: Blacksmith, Forge
            //Consumer: Blacksmith, Forge
        Ore,                //Producer: Mineshaft
            //Consumer: Blacksmith, Forge
        Coal,               //Producer: Mineshaft       //Note: Coal is preferred, burns longer, but is rarer
            //Consumer: Apothecary, Armorer, Bakery, Blacksmith, Candle Maker, Clay Pit, Distillery, Drydock, Forge, Gypsy, Jeweler, Tailor, Tar Kiln, Tavern
        Gunpowder,          //Producer: Host Continent, Alchemist
            //Consumer: Armory, Garrison, Hunter, Shipwright
        Weapons,            //Producer: Blacksmith
            //Consumer: Armory, Garrison, House
        Flour,              //Producer: Plantation, Mill
            //Consumer: Apothecary, Bakery, Barn, Shack, House
        Sugar,              //Producer: Plantation
            //Consumer: Apiary, Apothecary, Distillery, House, Market Stall, 
        Tools,              //Producer: Blacksmith, Forge
            //Consumer: Armorer, Bakery, Barber, Barn, Blacksmith, Butcher, Clay Pit, Construction Sites, Candle Marker, Carpenter, Cobbler, Fishing Hut, Forge, Hunter Shack, Jeweler Parlor, Leathersmith, Mill, Shipwright, Tailor
        Gold,               //Producer: Bank, Forge, Mine
            //Consumer: Apothecary, Bank, Blacksmith, Church, Forge, Gypsy Wagon, Jeweler Parlor
        Jewelry,            //Producer: Jeweler
            //Consumer: Church
        Medical_Supplies,   //Producer: Aphothecary, Gypsy
            //Consumer: Barber, Apothecary, Gypsy, Hunter
        Food,               //Producer: Bakery, Butcher, Shack, Market Stall
            //Consumer: Bawdy House, House, Shack, Tavern, Market Stall,
        Books,              //Producer: Host Continent, Sawmill
            //Consumer: Library, Church
        Raw_Meat,           //Producer: Hunter Shack, Shack
            //Consumer: Butcher, Gypsy Wagon, Market Stall, Shack, House, Tavern
        Fish,               //Producer: Fishing Hut, Shack, Lighthouse
            //Consumer: Butcher, Gypsy Wagon, Market Stall, Shack, House, Tavern
        Wax,                //Producer: Apiary
            //Consumer: Candle Maker, Shack, 
        Wool,               //Producer: Shack, Barn
            //Consumer: Shack, Tailor, House, Wig Maker
        Leather,            //Producer: Hunters Shack, Leathersmith
            //Consumer: Armorer, Blacksmith, Cobbler, Gypsy Wagon, Tailor
        Wheat,              //Producer: Plantation
            //Consumer: Mill
        Hops,               //Producer: Plantation
            //Consumer: Distillery
        Grapes,             //Producer: Plantation
            //Consumer: Market Stall, Distillery
        Alcohol,
        Clothing,
        Tar,                //Producer: Tar Kiln
            //Consumer: Shipwright, Drydock
        Rope,               //Producer: Shack, Tailor, Shipwright, Pawn Shop, 
            //Consumer: Shipwright, Drydock, Hunter Shack, Prison
        Skins,              //Producer: Shack, Hunters Shack
            //Consumer: Leathersmith, Cobbler, Gypsy
        Planks,             //Producer: Saw Mill
            //Consumer: Construction Sites, Shipwright, Drydock
        Bricks,             //Producer: Clay Pit
            //Consumer: Construction Sites
    }
    public CargoType cargoType;
    [SerializeField] private GameObject cargoShapePrefab;   //Crate, Barrel, Chest, Parcel
    public string markedForTransitByPawn;
}
