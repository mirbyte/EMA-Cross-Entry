using System;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;
// cTrader mirk0
// Github mirbyte

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class EMACrossEntryBot : Robot
    {
        [Parameter("Public v1.0", DefaultValue = "https://ctrader.com/users/profile/64575", Group = "m0")]
        public string M_ { get; set; }
        
        [Parameter("Period for EMA1 (signal)", DefaultValue = 50, Group = "Moving Averages")]
        public int EMA1Period { get; set; }

        [Parameter("Period for EMA2 (entry)", DefaultValue = 200, Group = "Moving Averages")]
        public int EMA2Period { get; set; }

        [Parameter("Volume", DefaultValue = 10000, MinValue = 10, Step = 1000, Group = "Volume")]
        public int Volume { get; set; }
        
        [Parameter("Multiplier Type (win or loss)", DefaultValue = "win", Group = "Volume")]
        public string MultType { get; set; }
        
        [Parameter("Volume Multiplier After Win/Loss (1 = off)", DefaultValue = 1, Group = "Volume")]
        public int Vm { get; set; }
        
        [Parameter("Multiplier After Second Win/Loss (1 = off)", DefaultValue = 1, Group = "Volume")]
        public int Vm2 { get; set; }

        [Parameter("Stop Loss (pips)", DefaultValue = 30, Group = "Position Protection")]
        public int StopLoss { get; set; }

        [Parameter("Take Profit (pips)", DefaultValue = 20, Group = "Position Protection")]
        public int TakeProfit { get; set; }
        
        [Parameter("Trailing Stoploss?", DefaultValue = false, Group = "Position Protection")]
        public bool IncludeTrailingStop { get; set; }

        [Parameter("Trailing Stop Trigger (pips)", DefaultValue = 2, Group = "Position Protection")]
        public double TrailingStopTrigger { get; set; }

        [Parameter("Trailing Stop Step (pips)", DefaultValue = 1, Group = "Position Protection")]
        public double TrailingStopStep { get; set; }

        private MovingAverage EMA1;
        private MovingAverage EMA2;

        private bool hasTraded = false;

        protected override void OnStart()
        {
            EMA1 = Indicators.MovingAverage(Bars.ClosePrices, EMA1Period, MovingAverageType.Exponential);
            EMA2 = Indicators.MovingAverage(Bars.ClosePrices, EMA2Period, MovingAverageType.Exponential);
        }

        protected override void OnTick()
        {
            if (IncludeTrailingStop)
            {
                SetTrailingStop();
            }

            if ((EMA1.Result.LastValue > EMA2.Result.LastValue && EMA1.Result.Last(1) < EMA2.Result.Last(1)) ||
                (EMA1.Result.LastValue < EMA2.Result.LastValue && EMA1.Result.Last(1) > EMA2.Result.Last(1)))
            {
                hasTraded = false;
            }
            double distance = Math.Abs(Symbol.Bid - EMA2.Result.LastValue);
            if (distance <= 1 * Symbol.PipSize)
            {
                if (EMA1.Result.LastValue > EMA2.Result.LastValue)
                {
                    if (Positions.Find("EMA cross long", SymbolName) == null && !hasTraded)
                    {
                        if (History.Count <= 1)
                        {
                            ExecuteMarketOrder(TradeType.Buy, SymbolName, Volume, "EMA cross long", StopLoss, TakeProfit);
                            hasTraded = true;
                        }
                        else if (History.Count > 1)
                        {
                            if (Positions.Find("EMA cross long", SymbolName) == null && Positions.Find("EMA cross short", SymbolName) == null && !hasTraded)
                            {
                                var lastTrade = History[History.Count - 1];
                                var last2Trade = History[History.Count - 2];
                                
                                /// win multiplier
                                if (lastTrade.Pips <= 0 && MultType=="win")
                                {
                                    ExecuteMarketOrder(TradeType.Buy, SymbolName, Volume, "EMA cross long", StopLoss, TakeProfit);
                                    hasTraded = true;
                                }
                                else if (lastTrade.Pips > 0 && MultType=="win")
                                {
                                    if (last2Trade.Pips > 0 && MultType=="win")
                                    {
                                        ExecuteMarketOrder(TradeType.Buy, SymbolName, Volume *Vm2, "EMA cross long", StopLoss, TakeProfit);
                                        hasTraded = true;
                                    }
                                    else if (last2Trade.Pips < 0 && MultType=="win")
                                    {
                                        ExecuteMarketOrder(TradeType.Buy, SymbolName, Volume *Vm, "EMA cross long", StopLoss, TakeProfit);
                                        hasTraded = true;
                                    }
                                }
                                /// loss multp
                                if (lastTrade.Pips >= 0 && MultType=="loss")
                                {
                                    ExecuteMarketOrder(TradeType.Buy, SymbolName, Volume, "EMA cross long", StopLoss, TakeProfit);
                                    hasTraded = true;
                                }
                                else if (lastTrade.Pips < 0 && MultType=="loss")
                                {
                                    if (last2Trade.Pips < 0 && MultType=="loss")
                                    {
                                        ExecuteMarketOrder(TradeType.Buy, SymbolName, Volume *Vm2, "EMA cross long", StopLoss, TakeProfit);
                                        hasTraded = true;
                                    }
                                    else if (last2Trade.Pips > 0 && MultType=="loss")
                                    {
                                        ExecuteMarketOrder(TradeType.Buy, SymbolName, Volume *Vm, "EMA cross long", StopLoss, TakeProfit);
                                        hasTraded = true;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (EMA1.Result.LastValue < EMA2.Result.LastValue)
                {
                    if (Positions.Find("EMA cross short", SymbolName) == null && !hasTraded)
                    {
                        if (History.Count <= 1) // ==
                        {
                            ExecuteMarketOrder(TradeType.Sell, SymbolName, Volume, "EMA cross short", StopLoss, TakeProfit);
                            hasTraded = true;
                        }
                        else if (History.Count > 1)
                        {
                            if (Positions.Find("EMA cross long", SymbolName) == null && Positions.Find("EMA cross short", SymbolName) == null && !hasTraded)
                            {
                                var lastTrade = History[History.Count - 1];
                                var last2Trade = History[History.Count - 2];
                                
                                /// win multip
                                if (lastTrade.Pips <= 0 && MultType=="win")
                                {
                                    ExecuteMarketOrder(TradeType.Sell, SymbolName, Volume, "EMA cross short", StopLoss, TakeProfit);
                                    hasTraded = true;
                                }
                                else if (lastTrade.Pips > 0 && MultType=="win")
                                {
                                    if (last2Trade.Pips > 0 && MultType=="win")
                                    {
                                        ExecuteMarketOrder(TradeType.Sell, SymbolName, Volume *Vm2, "EMA cross short", StopLoss, TakeProfit);
                                        hasTraded = true;
                                    }
                                    else if (last2Trade.Pips < 0 && MultType=="win")
                                    {
                                        ExecuteMarketOrder(TradeType.Sell, SymbolName, Volume *Vm, "EMA cross short", StopLoss, TakeProfit);
                                        hasTraded = true;
                                    }
                                }
                                /// loss multip
                                if (lastTrade.Pips >= 0 && MultType=="loss")
                                {
                                    ExecuteMarketOrder(TradeType.Sell, SymbolName, Volume, "EMA cross short", StopLoss, TakeProfit);
                                    hasTraded = true;
                                }
                                else if (lastTrade.Pips < 0 && MultType=="loss")
                                {
                                    if (last2Trade.Pips < 0 && MultType=="loss")
                                    {
                                        ExecuteMarketOrder(TradeType.Sell, SymbolName, Volume *Vm2, "EMA cross short", StopLoss, TakeProfit);
                                        hasTraded = true;
                                    }
                                    else if (last2Trade.Pips > 0 && MultType=="loss")
                                    {
                                        ExecuteMarketOrder(TradeType.Sell, SymbolName, Volume *Vm, "EMA cross short", StopLoss, TakeProfit);
                                        hasTraded = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void SetTrailingStop()
        {
            var sellPositions = Positions.FindAll("EMA cross short", SymbolName, TradeType.Sell);

            foreach (Position position in sellPositions)
            {
                double distance = position.EntryPrice - Symbol.Ask;

                if (distance < TrailingStopTrigger * Symbol.PipSize)
                    continue;

                double newStopLossPrice = Symbol.Ask + TrailingStopStep * Symbol.PipSize;

                if (position.StopLoss == null || newStopLossPrice < position.StopLoss)
                {
                    ModifyPosition(position, newStopLossPrice, position.TakeProfit);
                }
            }

            var buyPositions = Positions.FindAll("EMA cross long", SymbolName, TradeType.Buy);

            foreach (Position position in buyPositions)
            {
                double distance = Symbol.Bid - position.EntryPrice;

                if (distance < TrailingStopTrigger * Symbol.PipSize)
                    continue;

                double newStopLossPrice = Symbol.Bid - TrailingStopStep * Symbol.PipSize;
                if (position.StopLoss == null || newStopLossPrice > position.StopLoss)
                {
                    ModifyPosition(position, newStopLossPrice, position.TakeProfit);
                }
            }
        }
    }
}
// this is the ground level