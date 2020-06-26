package moe.yo3explorer.ag3dbRebuild.business.control;

import moe.yo3explorer.ag3dbRebuild.business.entity.DbCharacter;
import moe.yo3explorer.ag3dbRebuild.business.entity.ExtractedRating;
import org.apache.commons.math3.distribution.ExponentialDistribution;
import org.apache.commons.math3.distribution.PoissonDistribution;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.jetbrains.annotations.Contract;
import org.jetbrains.annotations.NotNull;

import java.util.*;
import java.util.stream.Collectors;

public class RatingGuesser
{
    private static Logger logger;

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
        {
            target = character.rating;
        }

        if (target == 0)
            return Collections.emptyList();

        int[] ratings = search(character.rating);
        List<ExtractedRating> collect = Arrays.stream(ratings).mapToObj(x -> ExtractedRating.createGuess(character, x)).collect(Collectors.toList());
        return collect;
    }

    private static double round(double value)
    {
        return (double)Math.round(value * 100000d) / 100000d;
    }

    private int[] searchState;
    private HashMap<Double,int[]> searchCache;

    private int[] search(double target)
    {
        if (searchCache == null)
            searchCache = new HashMap<>();
        if (searchCache.containsKey(target))
            return searchCache.get(target);

        double searchStateAvg = Double.NaN;
        logger.info(String.format("Searching for %f",target));
        while (searchStateAvg != target)
        {
            incrementSearchState(0);
            searchStateAvg = fastAverage(searchState);
            searchStateAvg = round(searchStateAvg);
            if (!searchCache.containsKey(searchStateAvg)) {
                int[] newChain = Arrays.copyOf(searchState, searchState.length);
                logger.info(String.format("Found chain for %.5f: (%s)",searchStateAvg,String.join("->", Arrays.stream(newChain).mapToObj(x -> Integer.toString(x)).collect(Collectors.toList()))));
                searchCache.put(searchStateAvg, newChain);
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
        return d;
    }

    private void incrementSearchState(int offset)
    {
        if (searchState == null)
        {
            searchState = new int[]{1};
            return;
        }
        if (searchState.length < offset + 1)
        {
            int[] increasedRoom = Arrays.copyOf(searchState,offset + 1);
            increasedRoom[offset] = 1;
            searchState = increasedRoom;
            logger.info(String.format("Increased search space to %d!",offset));
            return;
        }

        searchState[offset]++;
        if (searchState[offset] == 6)
        {
            searchState[offset] = 1;
            incrementSearchState(offset + 1);
        }
    }
}
