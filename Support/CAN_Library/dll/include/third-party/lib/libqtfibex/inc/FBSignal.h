#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FibexTypes.h"
#include "FBAttribute.h"

#include <QDebug>

namespace ASAM {
namespace FIBEX {

class FBPdu;
class FBSignalGroup;
class FXSignalType;
typedef FibexTypes::FXBitCountingPolicy ByteOrder;
typedef FibexTypes::HOBaseDataType CodedType;
typedef FibexTypes::FXTypeTypeSt SignalType;
// @todo fibex textable item should mapping to a range
typedef QMap<double, QString> ValueDescriptions;

class ASAM_FIBEX_EXPORT FBSignal
{
public:
    FBSignal();

    QString name() const;
    QString description() const;
    bool getDefaultValue(double &v) const;
    bool isSignalType(FibexTypes::FXTypeTypeSt type) const;
    void setFrameUpdateBit(char *frameData) const;

    const FXSignalType *m_fxSignal = nullptr;
    quint32 *m_signalUpdateBitPosition = nullptr;
    SignalType *m_signalType = nullptr;
    FBPdu *m_fbPdu = nullptr;
    FBSignalGroup *m_fbSignalGroup = nullptr;
    bool m_isGrpHighLowByteOrder;
    quint16 m_grpBitPosition;

    /** Multiplexed Signal (m) */
    bool m_isMultiplexedSignal; // m

    /** Multiplexer Switch Value */
    unsigned int m_multiplexerSwitchValue;

    /** Multiplexor Switch (M) */
    bool m_isMultiplexorSwitch; // M

    quint16 m_bitPosition;

    /** Start Bit, always at the (LSB)least significant bit position */
    quint16 m_startBit;

    /** Bit Size */
    quint16 m_bitSize;

    /** Byte Order */
    bool m_isHighLowByteOrder;

    /** Value Type */
    CodedType m_codedType;

    /** Factor */
    double m_factor;

    /** Offset */
    double m_offset;

    /** Minimun Physical Value (or 0 if auto calculated) */
    double m_minimumPhysicalValue;

    /** Maximum Physical Value (or 0 if auto calculated) */
    double m_maximumPhysicalValue;

    /** Unit */
    QString m_unit;

    /** Receivers */
    QStringList m_receivers;

    /** Signal Extended Value Type (SIG_VALTYPE, obsolete) */
    enum class ExtendedValueType : char {
        Undefined = ' ',
        Integer = '0',
        Float = '1',
        Double = '2'
    };

    /** Signal Extended Value Type (SIG_VALTYPE, obsolete) */
    ExtendedValueType m_extendedValueType;

    /** Value Descriptions (VAL) */
    ValueDescriptions m_valueDescriptions;

    /** Signal Type Refs (SGTYPE, obsolete) */
    QString m_type;

    /** Comment (CM) */
    QString m_comment;

    /** Raw Value of Signal */
    double m_rawValue;

	/** Phy Value of Signal */
    double m_phyValue;

    /** Attribute Values (BA) */
    QMap<QString, FBAttribute> m_attributeValues;

    /** Extended Multiplexors (SG_MUL_VAL) */
    //QList<ExtendedMultiplexor> extendedMultiplexors;

    //Message *parentMessage;

    /**
     * @brief Convert from Raw to Physical Value
     * @param[in] rawValue Raw Value
     * @return Raw Value
     *
     * Converts a value from raw to physical representation.
     */
    double rawToPhysicalValue(double rawValue) const;

    /**
     * @brief Convert from Physical to Raw Value
     * @param[in] physicalValue Physical Value
     * @return Physical Value
     *
     * Converts a value from physical to raw representation.
     */
    double physicalToRawValue(double physicalValue) const;

    /**
     * @brief Decodes/Extracts a signal from the message data
     * @param[in] data Data
     * @return Raw signal value
     *
     * Decodes/Extracts a signal from the message data.
     *
     * @note Multiplexors are not taken into account.
     */
    uint64_t decode(const uint8_t *msgData) const;

    /**
     * @brief Encodes a signal into the message data
     * @param[inout] data Data
     * @param[in] rawValue Raw signal value
     *
     * Encode a signal into the message data.
     *
     * @note Multiplexors are not taken into account.
     */
    void encode(uint8_t *msgData, uint64_t rawValue) const;

    QStringList getValueDescriptionTextAll() const;
    QString getValueDescriptionText(double rawValue) const;
    bool getValueDescriptionValue(const QString &value, double &v) const;

    bool processAsText(const uint8_t *msgData, QString &outString) const;
    bool processAsInt(const uint8_t *msgData, int32_t &outValue) const;
    bool processAsDouble(const uint8_t *msgData, double &outValue) const;
    bool getTextValues(const uint8_t *msgData, QString &valueString, QString &descriptionString) const;
	double minimumRawValue() const;
	double maximumRawValue() const;
    double minimumPhyValue(bool isOutputDirectly = false) const;
    double maximumPhyValue(bool isOutputDirectly = false) const;
	
    void encodePhy(uint8_t *msgData, double phyValue) const;
    double decodePhy(const uint8_t *msgData) const;

    bool setStartBitByPosition(quint16 bitPosition);
    static quint16 getStartBitByPosition(quint16 bitPosition,
                                         bool isHighLowByteOrder,
                                         quint16 bitSize);
};

QDebug inline operator<<(QDebug d, const FBSignal &s)
{
    double v;

    QDebugStateSaver saver(d);
    d.nospace().noquote() << "FBSignal: ("
      << "name:" << s.name() << ","
      << "ftr:" << s.m_factor << ","
      << "off:" << s.m_offset << ","
      << "ct:" << static_cast<int>(s.m_codedType) << ","
      << "st:" << (s.m_signalType?(QString::number(static_cast<int>(*s.m_signalType))):"N/A") << ","
      << "min:" << s.m_minimumPhysicalValue << ","
      << "max:" << s.m_maximumPhysicalValue << ","
      << "sb:" << s.m_startBit << ","
      << "sz:" << s.m_bitSize << ","
      << "big:" << s.m_isHighLowByteOrder << ","
      << "upb:" << (s.m_signalUpdateBitPosition?(QString::number(*s.m_signalUpdateBitPosition)):"N/A") << ","
      << "def:" << ((s.getDefaultValue(v))?(QString::number(v)):"N/A") << ","
      << "unit:" << s.m_unit << ","
      << "txt:" << s.m_valueDescriptions
      << ")";

    return d;
}

}
}
