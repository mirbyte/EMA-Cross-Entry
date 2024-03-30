# EMA Cross Entry cBot
Sophisticated EMA crossover cBot. Below is a list of key components:

### Moving Averages Setup:
EMA1: Acts as a signal line.
EMA2: Used for entry.
### Volume Management:
Volume: Adjust position size.
Multiplier Type: Specifies whether to use volume multipliers after wins or losses. It can be set only to "win" or “loss”.
Volume multiplier: “1” to turn off multiplier as you may have guessed.
### Position Protection:
StopLoss
TakeProfit
Trailing Stoploss
### Trading Logic:
Checks for EMA crossovers and executes trades accordingly.
The algorithm includes logic to prevent multiple trades within a short time using the hasTraded flag.
The SetTrailingStop method is used to implement a trailing stop feature for both long and short positions.
### Disclaimer:
Trading algorithms involve risk, and this script should be thoroughly tested in a simulated environment before using it in a live trading scenario. Users should understand the parameters and logic to adapt them to their risk tolerance and market conditions. **I take no responsibility for your losses.**

![ema cross entry](https://github.com/mirbyte/cBot-EMA-Cross-Entry/assets/83219244/7617add5-dbd8-42fe-8abd-1349364dbc12)
![parameters](https://github.com/mirbyte/cBot-EMA-Cross-Entry/assets/83219244/7bcba0ed-de92-459b-93ce-c7fd391d61a6)


_I prioritize updates on the cTrader website._
