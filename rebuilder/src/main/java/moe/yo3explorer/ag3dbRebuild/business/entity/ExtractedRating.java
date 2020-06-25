package moe.yo3explorer.ag3dbRebuild.business.entity;

import org.jetbrains.annotations.NotNull;

public class ExtractedRating
{
    public int ratingValue;
    public int userId;
    public int characterId;

    public static @NotNull ExtractedRating createGuess(@NotNull DbCharacter character, int rating)
    {
        ExtractedRating result = new ExtractedRating();
        result.ratingValue = rating;
        result.userId = -1;
        result.characterId = character.id;
        return result;
    }
}
