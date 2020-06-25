package moe.yo3explorer.ag3dbRebuild.business.control;

import moe.yo3explorer.ag3dbRebuild.business.entity.DbCharacter;
import moe.yo3explorer.ag3dbRebuild.business.entity.ExtractedRating;
import org.apache.commons.math3.distribution.ExponentialDistribution;

import java.util.Collections;
import java.util.LinkedList;
import java.util.List;

public class RatingGuesser
{
    private int slips;
    public List<ExtractedRating> generateRatings(DbCharacter character)
    {
        slips = 0;
        int maxRatings = Math.max(30,character.hits);

        double target = round(character.rating);
        double current = Double.NaN;
        double edMean = round(character.rating / 10d);

        if (target == 0)
            return Collections.emptyList();

        LinkedList<ExtractedRating> ratings = new LinkedList<>();
        ExponentialDistribution exponentialDistribution = new ExponentialDistribution(edMean,1e-5);

        do {
            if (ratings.size() > maxRatings) {
                ratings.clear();
                slips++;
            }
            double sample = exponentialDistribution.sample();
            sample *= 10d;
            sample = round(sample);
            int tba = (int)sample;
            tba = clamp(tba,1,5);
            ratings.add(ExtractedRating.createGuess(character,tba));
            current = ratings.stream().mapToDouble(x -> (double)x.ratingValue).average().getAsDouble();
            current = round(current);
        } while (target != current);
        return ratings;
    }

    private static double round(double value)
    {
        return (double)Math.round(value * 100000d) / 100000d;
    }

    private static int clamp(int actual, int lower, int upper)
    {
        if (lower > actual)
            return lower;
        if (upper < actual)
            return upper;
        return actual;
    }

    public int getSlips() {
        return slips;
    }

}
