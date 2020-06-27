package moe.yo3explorer.ag3dbRebuild.business.control;

import moe.yo3explorer.ag3dbRebuild.business.entity.DbCharacter;
import moe.yo3explorer.ag3dbRebuild.business.entity.ExtractedRating;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.jetbrains.annotations.Contract;
import org.jetbrains.annotations.NotNull;

import java.util.*;
import java.util.stream.Collectors;

public class RatingGuesser
{
    private static Logger logger;
    private int highestSearchDepth;

    public RatingGuesser()
    {
        if (logger == null)
        {
            logger = LogManager.getLogger(getClass());
            logger.info("RatingGuesser has joined the action!");
        }
    }

    public List<ExtractedRating> generateRatings(@NotNull DbCharacter character)
    {
        double target = round(character.rating);
        if (Double.isNaN(target))
            target = character.rating;

        if (target == 0)
            return Collections.emptyList();

        int[] ratings = search(character.rating);
        List<ExtractedRating> collect = Arrays.stream(ratings).mapToObj(x -> ExtractedRating.createGuess(character, (int)x)).collect(Collectors.toList());
        return collect;
    }

    private static double round(double value)
    {
        return (double)Math.round(value * 100000d) / 100000d;
    }

    private int[] searchState;
    private int[] search(double target)
    {
        double[] ring = new double[] {Double.NaN, Double.NaN};
        searchState = new int[] {1};
        double searchStateAvg = 1;
        if (target == 1.0)
            return searchState;
        while (searchStateAvg != target)
        {
            if (searchStateAvg > target)
                decreaseState();
            else if (searchStateAvg < target)
                increaseState();
            searchStateAvg = fastAverage(searchState);
            if (searchStateAvg == ring[0])
            {
                ring[0] = ring[1] = Double.NaN;
                increaseSearchRoom();
                searchState[searchState.length - 1] = 1;
                searchStateAvg = fastAverage(searchState);
            }
            else {
                ring[0] = ring[1];
                ring[1] = searchStateAvg;
            }
        }
        return searchState;
    }

    @Contract(pure = true)
    private double fastAverage(int @NotNull [] values)
    {
        double d = 0;
        for (int i = 0; i < values.length; i++)
        {
            d += values[i];
        }
        d /= values.length;
        d = round(d);
        return d;
    }

    private void increaseSearchRoom() {
        int[] increasedRoom = Arrays.copyOf(searchState,searchState.length + 1);
        searchState = increasedRoom;
        if (highestSearchDepth < increasedRoom.length)
        {
            logger.info(String.format("Increased search room to %d",increasedRoom.length));
            highestSearchDepth = increasedRoom.length;
        }
    }

    private void increaseState()
    {
        for (int i = 0; i < searchState.length; i++)
        {
            if (searchState[i] != 5) {
                searchState[i]++;
                return;
            }
        }
        throw new RuntimeException("failed to increase search state!");
    }

    private void decreaseState()
    {
        for (int i = 0; i < searchState.length; i++)
        {
            if (searchState[i] != 1) {
                searchState[i]--;
                return;
            }
        }
        throw new RuntimeException("failed to decrease search state!");
    }
}
