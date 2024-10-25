package com.trev3d;

import android.graphics.Point;

import java.io.Serializable;

public class BarcodeResult implements Serializable {
	public String text;
	public int format;
//	public Point[] points;
	public long timestamp;
}
