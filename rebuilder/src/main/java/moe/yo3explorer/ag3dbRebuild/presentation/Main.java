package moe.yo3explorer.ag3dbRebuild.presentation;

import moe.yo3explorer.ag3dbRebuild.business.boundary.Ag3DbOrm;
import moe.yo3explorer.ag3dbRebuild.business.boundary.UserParseService;
import moe.yo3explorer.ag3dbRebuild.business.control.RatingGuesser;
import moe.yo3explorer.ag3dbRebuild.business.entity.DbCharacter;
import moe.yo3explorer.ag3dbRebuild.business.entity.DbUser;
import moe.yo3explorer.ag3dbRebuild.business.entity.ExtractedCharacter;
import org.apache.commons.compress.archivers.tar.TarArchiveEntry;
import org.apache.commons.compress.archivers.tar.TarArchiveInputStream;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.jetbrains.annotations.NotNull;

import java.io.*;
import java.sql.SQLException;
import java.util.List;

public class Main
{
    public static void main(String @NotNull [] args) throws IOException, SQLException {
        Logger logger = LogManager.getLogger(Main.class);
        if (args.length == 0)
        {
            logger.error("Pass the tar file as an argument!");
            return;
        }

        File infile = new File(args[0]);
        if (!infile.isFile())
        {
            logger.error(String.format("%s was not found!",args[0]));
            return;
        }

        Ag3DbOrm orm = null;
        RatingGuesser ratingGuesser = null;
        FileInputStream level1 = new FileInputStream(infile);
        TarArchiveInputStream level2 = new TarArchiveInputStream(level1);
        TarArchiveEntry tarEntry = null;
        while ((tarEntry = level2.getNextTarEntry()) != null)
        {
            if (tarEntry.isDirectory())
                continue;
            if (!tarEntry.getName().startsWith("chars/"))
                continue;
            if (!tarEntry.getName().toLowerCase().endsWith(".7z"))
                continue;

            int size = (int)tarEntry.getSize();
            byte[] tarBuffer = new byte[size];
            level2.read(tarBuffer,0,size);

            UserParseService foundUser = UserParseService.parseUser(tarEntry, tarBuffer);
            List<ExtractedCharacter> extractedCharacters = foundUser.extractCharacters();
            if (extractedCharacters.size() == 0)
                continue;

            for (ExtractedCharacter extractedCharacter : extractedCharacters) {
                if (orm == null)
                    orm = new Ag3DbOrm();

                DbCharacter dbCharacterByCrc32 = orm.findDbCharacterByCrc32(extractedCharacter.crc32);
                if (dbCharacterByCrc32 == null)
                {
                    logger.warn(String.format("CRC32 %s found in %s, but not in SQL dump.",extractedCharacter.crc32,tarEntry.getName()));
                    continue;
                }
                boolean alreadyInserted = orm.testForCharacterData(dbCharacterByCrc32.id);
                if (alreadyInserted)
                    continue;

                DbUser dbUser = orm.autoGetDbUser(dbCharacterByCrc32, foundUser);
                if (ratingGuesser == null)
                    ratingGuesser = new RatingGuesser();
                extractedCharacter.ratings = ratingGuesser.generateRatings(dbCharacterByCrc32);

                orm.postCharacter(extractedCharacter,dbCharacterByCrc32.id);
            }

        }
    }
}
