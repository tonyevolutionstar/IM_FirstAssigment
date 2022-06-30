/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package genfusionscxml;

import java.io.IOException;
import scxmlgen.Fusion.FusionGenerator;
import scxmlgen.Modalities.Output;
import scxmlgen.Modalities.Speech;
import scxmlgen.Modalities.SecondMod;

/**
 *
 * @author nunof
 */
public class GenFusionSCXML {

    /**
     * @param args the command line arguments
     */
    public static void main(String[] args) throws IOException {

    FusionGenerator fg = new FusionGenerator();
    fg.Single(Speech.PLAY, Output.PLAY);
    fg.Single(Speech.NEXT, Output.NEXT);
    fg.Single(Speech.PREV, Output.PREV);
    fg.Single(Speech.PAUSE, Output.PAUSE);
    fg.Single(Speech.RESUME, Output.RESUME);
    fg.Single(Speech.VOLUP, Output.VOLUP);
    fg.Single(Speech.FAST, Output.FAST);
    fg.Single(Speech.VOLDOWN, Output.VOLDOWN);  
    fg.Single(Speech.SLOW, Output.SLOW);
    fg.Single(Speech.REPEATON, Output.REPEATON);
    fg.Single(Speech.REPEATOFF, Output.REPEATOFF);
    fg.Single(Speech.RANDOMON, Output.RANDOMON);
    fg.Single(Speech.RANDOMOFF, Output.RANDOMOFF);
    fg.Single(Speech.QUIT, Output.QUIT);    
    fg.Single(Speech.MUTE, Output.MUTE);   
    fg.Single(Speech.MUSIC_NAME, Output.MUSIC_NAME);
    fg.Single(Speech.TIME_MUSIC, Output.TIME_MUSIC);
    
    fg.Single(SecondMod.next, Output.NEXT);
    fg.Single(SecondMod.prev, Output.PREV);
    fg.Single(SecondMod.mute, Output.MUTE);
    
    
    fg.Redundancy(Speech.NEXT, SecondMod.next, Output.NEXT);
    fg.Redundancy(Speech.PREV, SecondMod.prev, Output.PREV);
    fg.Redundancy(Speech.MUTE, SecondMod.mute, Output.MUTE);
    fg.Complementary(Speech.FULLNUMBER, SecondMod.volumemusicL, Output.VOLUME);

    
    fg.Build("fusion.scxml");
             
    }
}