package moe.yo3explorer.ag3dbRebuild.business.control;

import moe.yo3explorer.ag3dbRebuild.business.boundary.Ag3DbOrm;
import moe.yo3explorer.ag3dbRebuild.business.entity.DbCharacter;
import moe.yo3explorer.ag3dbRebuild.business.entity.ExtractedRating;
import org.junit.Test;

import java.io.IOException;
import java.sql.SQLException;
import java.util.List;

public class RatingGuesserTests {
    @Test
    public void testRatingGenerator() throws IOException, SQLException {
        Ag3DbOrm ag3DbOrm = new Ag3DbOrm();
        DbCharacter firstRowCharacter = ag3DbOrm.findFirstRowCharacter();
        RatingGuesser ratingGuesser = new RatingGuesser();
        List<ExtractedRating> extractedRatings = ratingGuesser.generateRatings(firstRowCharacter);
        for (ExtractedRating extractedRating : extractedRatings) {
            System.out.print(extractedRating.ratingValue);
            System.out.print(',');
        }
        System.out.println();
        System.out.printf("%d slips\n",ratingGuesser.getSlips());

    }
}
