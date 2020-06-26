package moe.yo3explorer.ag3dbRebuild.business.control;

import moe.yo3explorer.ag3dbRebuild.business.boundary.Ag3DbOrm;
import moe.yo3explorer.ag3dbRebuild.business.entity.DbCharacter;
import moe.yo3explorer.ag3dbRebuild.business.entity.ExtractedRating;
import org.jetbrains.annotations.NotNull;
import org.junit.Test;

import java.io.IOException;
import java.sql.SQLException;
import java.util.List;

public class RatingGuesserTests {
    @Test
    public void testRatingGenerator() throws IOException, SQLException {
        RatingGuesser ratingGuesser = new RatingGuesser();
        ratingGuesser.generateRatings(buildDummyCharacter(4.57576));
        ratingGuesser.generateRatings(buildDummyCharacter(4.3));
        ratingGuesser.generateRatings(buildDummyCharacter(4.29412));
        ratingGuesser.generateRatings(buildDummyCharacter(4.1875));
        ratingGuesser.generateRatings(buildDummyCharacter(4.16667));
        ratingGuesser.generateRatings(buildDummyCharacter(4.14286));
        ratingGuesser.generateRatings(buildDummyCharacter(4));
        ratingGuesser.generateRatings(buildDummyCharacter(3.88889));
        ratingGuesser.generateRatings(buildDummyCharacter(3.85714));
        ratingGuesser.generateRatings(buildDummyCharacter(3.83333));
        ratingGuesser.generateRatings(buildDummyCharacter(3.82353));
        ratingGuesser.generateRatings(buildDummyCharacter(3.8));
        ratingGuesser.generateRatings(buildDummyCharacter(3.75));
        ratingGuesser.generateRatings(buildDummyCharacter(2.875));
        ratingGuesser.generateRatings(buildDummyCharacter(2.33333));
        ratingGuesser.generateRatings(buildDummyCharacter(2.25));
        ratingGuesser.generateRatings(buildDummyCharacter(2.2));
        ratingGuesser.generateRatings(buildDummyCharacter(2.11111));
        ratingGuesser.generateRatings(buildDummyCharacter(1.8));
        ratingGuesser.generateRatings(buildDummyCharacter(1.75));
        ratingGuesser.generateRatings(buildDummyCharacter(1.6));
        ratingGuesser.generateRatings(buildDummyCharacter(1));
    }

    private @NotNull DbCharacter buildDummyCharacter(double rating)
    {
        DbCharacter dbCharacter = new DbCharacter();
        dbCharacter.rating = rating;
        return dbCharacter;
    }
}
