package scxmlgen.Modalities;

import scxmlgen.interfaces.IOutput;



public enum Output implements IOutput{
    PLAY("[PLAY]"),
    NEXT("[NEXT]"),
    PREV("[PREV]"),
    PAUSE("[PAUSE]"),
    RESUME("[RESUME]"),
    VOLUP("[VOLUP]"),
    FAST("[FAST]"),
    VOLDOWN("[VOLDOWN]"),
    SLOW("[SLOW]"),
    REPEATON("[REPEATON]"),
    REPEATOFF("[REPEATOFF]"),
    RANDOMON("[RANDOMON]"),
    RANDOMOFF("[RANDOMOFF]"),
    QUIT("[QUIT]"),
    MUTE("[MUTE]"),
    SPEAK("[SPEAK]"),
    MUSIC_NAME("[MUSIC_NAME]"),
    TIME_MUSIC("[TIME_MUSIC]"),
    VOLUME("[fullnumber]");

    private String event;

    Output(String m) {
        event=m;
    }
    
    public String getEvent(){
        return this.toString();
    }

    public String getEventName(){
        return event;
    }
}
