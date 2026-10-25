#include "FXSignalType.h"
#include "FBSignal.h"
#include "FBSignalGroup.h"

//#include "Utility.h"
//using Vector::DBC::Utility;

#include <QtMath>

#define SETBIT(dest,bit)	( *( reinterpret_cast<quint8 *>(dest)    + (bit / 8) ) |= static_cast<quint8>(1u << (bit % 8)) )
#define CLEARBIT(dest,bit)	( *( reinterpret_cast<quint8 *>(dest)    + (bit / 8) ) &= static_cast<quint8>(~static_cast<quint8>(1u << (bit % 8))) )
#define TESTBIT(source,bit)	( *( reinterpret_cast<const quint8 *>(source)  + (bit / 8) ) &  static_cast<quint8>(1u << (bit % 8)) )

namespace ASAM {
namespace FIBEX {

FBSignal::FBSignal()
    : m_isMultiplexedSignal(false)
    , m_multiplexerSwitchValue(0)
    , m_isMultiplexorSwitch(false)

    /* position */
    , m_startBit(0)
    , m_bitSize(0)
    , m_isHighLowByteOrder(true)
    , m_codedType(CodedType::A_UINT64)

    /* raw/physical conversion */
    , m_factor(1.0)
    , m_offset(0.0)
    , m_minimumPhysicalValue(0.0)
    , m_maximumPhysicalValue(0.0)

    /* unit */
    , m_unit("Unitless")

    /* receivers */
    , m_receivers()

    /* value type and description */
    , m_extendedValueType(FBSignal::ExtendedValueType::Undefined)
    //valueDescriptions(),
    , m_type()

    /* comments and attributes */
    , m_comment()
    , m_attributeValues()

    /* extended multiplexors */
    //extendedMultiplexors()
{
    /* nothing to do here */
}



QString FBSignal::name() const
{
    return m_fxSignal?m_fxSignal->m_shortName:QString();
}

QString FBSignal::description() const
{
    return m_fxSignal?m_fxSignal->m_desc:QString();
}

bool FBSignal::getDefaultValue(double &v) const
{
    if (m_fxSignal && m_fxSignal->m_defaultValue) {
        v = *m_fxSignal->m_defaultValue;
        return true;
    }
    return false;
}

bool FBSignal::isSignalType(FibexTypes::FXTypeTypeSt type) const
{
    return (m_signalType && (*m_signalType == type))?true:false;
}

void FBSignal::setFrameUpdateBit(char *frameData) const
{
    if (m_fbSignalGroup) {
        if (m_fbSignalGroup->m_updateBitPosition) {
            SETBIT(frameData, *m_fbSignalGroup->m_updateBitPosition);
        }
    } else {
        if (m_signalUpdateBitPosition) {
            SETBIT(frameData, *m_signalUpdateBitPosition);
        }
    }
}

bool FBSignal::processAsText(const uint8_t *msgData, QString &outString) const
{
    uint64_t result = 0;
    double phyValue;

    if (FibexTypes::isDataTypeString(m_codedType)) {
        QString buildString;
        int startByte = m_startBit / 8;
        int bytes = m_bitSize / 8;
        for (int x = 0; x < bytes; x++) buildString.append(msgData[startByte + x]);
        outString = buildString;
        return true;
    }

    //if this is a multiplexed signal then we have to see if it is even found in the current message
    if (m_isMultiplexedSignal) {
        // don't support
        return false;
    }

    result = decode(msgData);
    phyValue = (static_cast<double>(result) * m_factor) + m_offset;

    QString outputString;
    outputString = name() + ": ";

    if (m_valueDescriptions.count() > 0) { //if this is a value list type then look it up and display the proper string
        auto it = m_valueDescriptions.find(result);
        if ((it != m_valueDescriptions.end())
                && (static_cast<uint64_t>(it.key()) == result)) {
            outputString += it.value();
        }
    } else { //otherwise display the actual number and unit (if it exists)
       outputString += QString::number(phyValue, 'f', 2) + " " + m_unit;
    }

    outString = outputString;
    return true;
}

bool FBSignal::processAsInt(const uint8_t *msgData, int32_t &outValue) const
{
    uint64_t result = 0;

    if (!FibexTypes::isDataTypeInteger(m_codedType)) {
        return false;
    }

    //if this is a multiplexed signal then we have to see if it is even found in the current message
    if (m_isMultiplexedSignal) {
        // don't support
        return false;
    }

    result = decode(msgData);
    double endResult = (static_cast<double>(result) * m_factor) + m_offset;
    outValue = static_cast<int32_t>(endResult);

    return true;
}

bool FBSignal::processAsDouble(const uint8_t *msgData, double &outValue) const
{
    uint64_t result = 0;

    if (FibexTypes::isDataTypeString(m_codedType)) {
        return false;
    }

    //if this is a multiplexed signal then we have to see if it is even found in the current message
    if (m_isMultiplexedSignal) {
        // don't support
        return false;
    }

    result = decode(msgData);
    outValue = (static_cast<double>(result) * m_factor) + m_offset;

    return true;
}

bool FBSignal::getTextValues(const uint8_t *msgData, QString &valueString, QString &descriptionString) const
{
    uint64_t result = 0;
    double phyValue;

    valueString.clear();
    descriptionString.clear();

    if (FibexTypes::isDataTypeString(m_codedType)) {
        QString buildString;
        int startByte = m_startBit / 8;
        int bytes = m_bitSize / 8;
        for (int x = 0; x < bytes; x++) buildString.append(msgData[startByte + x]);
        valueString = buildString;
        return true;
    }

    //if this is a multiplexed signal then we have to see if it is even found in the current message
    if (m_isMultiplexedSignal) {
        // don't support
        return false;
    }

    result = decode(msgData);
    phyValue = (static_cast<double>(result) * m_factor) + m_offset;
    valueString = QString::number(phyValue, 'f', 2);

    if (m_valueDescriptions.count() > 0) { //if this is a value list type then look it up and display the proper string
        auto it = m_valueDescriptions.find(result);
        if ((it != m_valueDescriptions.end())
                && (static_cast<uint64_t>(it.key()) == result)) {
            descriptionString = it.value();
        }
    }

    return true;
}

double FBSignal::rawToPhysicalValue(double rawValue) const
{
    /* physicalValue = rawValue * factor + offset */
    return rawValue * m_factor + m_offset;
}

double FBSignal::physicalToRawValue(double physicalValue) const
{
    /* safety check */
    if (m_factor == 0.0)
        return 0;

    /* rawValue = (physicalValue - offset) / factor */
    return (physicalValue - m_offset) / m_factor;
}

double FBSignal::minimumPhyValue(bool isOutputDirectly) const
{
    if ((m_minimumPhysicalValue != 0.0) || (isOutputDirectly))
        return m_minimumPhysicalValue;
	else
		return rawToPhysicalValue(minimumRawValue());
}

double FBSignal::maximumPhyValue(bool isOutputDirectly) const
{
    if ((m_maximumPhysicalValue != 0.0) || (isOutputDirectly))
        return m_maximumPhysicalValue;
	else
		return rawToPhysicalValue(maximumRawValue());
}

double FBSignal::minimumRawValue() const
{
    /* calculate minimum raw value */
    double minimumRawValue = 0.0;
	
    switch (m_codedType) {
        case CodedType::A_UINT8:
        case CodedType::A_UINT16:
        case CodedType::A_UINT32:
        case CodedType::A_UINT64:
            minimumRawValue = 0.0;
            break;
        case CodedType::A_INT8:
        case CodedType::A_INT16:
        case CodedType::A_INT32:
        case CodedType::A_INT64:
            minimumRawValue = -(2<<(m_bitSize-2));
            break;
        case CodedType::A_FLOAT32:
        	minimumRawValue = 3.4e-38;
        	break;

        case CodedType::A_FLOAT64:
        	minimumRawValue = 1.7e-308;
        	break;

		default:
			break;
    }
	
    return minimumRawValue;
}

double FBSignal::maximumRawValue() const
{
    /* calculate maximum raw value */
    double maximumRawValue = 0.0;
	
    switch (m_codedType) {
        case CodedType::A_UINT8:
        case CodedType::A_UINT16:
        case CodedType::A_UINT32:
        case CodedType::A_UINT64:
            maximumRawValue = (m_bitSize<1)?0:(2<<(m_bitSize-1))-1;
            break;
        case CodedType::A_INT8:
        case CodedType::A_INT16:
        case CodedType::A_INT32:
        case CodedType::A_INT64:
            maximumRawValue = (m_bitSize<2)?0:(2<<(m_bitSize-2))-1;
            break;
        case CodedType::A_FLOAT32:
        	maximumRawValue = 3.4e38;
        	break;

        case CodedType::A_FLOAT64:
        	maximumRawValue = 1.7e308;
        	break;

		default:
			break;
    }
    return maximumRawValue;
}

uint64_t FBSignal::decode(const uint8_t *msgData) const
{
    std::uint64_t retVal = 0;
    unsigned int size = m_bitSize;

    /* copy bits, all start with LSB (low byte low bit) */
    if (m_isHighLowByteOrder) {
        unsigned int srcBit = m_startBit;
        unsigned int dstBit = 0;
        while(size > 0) {
            /* copy bit */
            if (msgData[srcBit/8] & (1<<(srcBit%8))) {
                retVal |= (1ULL<<dstBit);
            }

            /* calculate next position */
            ++srcBit;
            if ((srcBit % 8) == 0) {
                srcBit -= 16;
            }

            ++dstBit;
            --size;
        }
    } else {
        unsigned int srcBit = m_startBit;
        unsigned int dstBit = 0;
        while(size > 0) {
            /* copy bit */
            if (msgData[srcBit/8] & (1<<(srcBit%8))) {
                retVal |= (1ULL<<dstBit);
            }

            /* calculate next position */
            ++srcBit;
            ++dstBit;
            --size;
        }
    }

    /* if signed, then fill all bits above MSB with 1 */
    if (FibexTypes::isDataTypeSignedNumber(m_codedType)) {
        uint64_t msb = (retVal >> (size - 1)) & 1;
        if (msb) {
            for (unsigned int i = size; i < 8*sizeof(retVal); ++i) {
                retVal |= (1ULL<<i);
            }
        }
    }

    return retVal;
}

void FBSignal::encode(uint8_t *msgData, uint64_t rawValue) const
{
    unsigned int size = m_bitSize;

    /* copy bits, all start with LSB (low byte low bit) */
    if (m_isHighLowByteOrder) {
        unsigned int srcBit = m_startBit;
        unsigned int dstBit = 0;
        while(size > 0) {
            /* copy bit */
            if (rawValue & (1ULL<<dstBit)) {
                msgData[srcBit/8] |= (1<<(srcBit%8));
            } else {
                msgData[srcBit/8] &= ~(1<<(srcBit%8));
            }

            /* calculate next position */
            ++srcBit;
            if ((srcBit % 8) == 0) {
                srcBit -= 16;
            }

            ++dstBit;
            --size;
        }
    } else {
        unsigned int srcBit = m_startBit;
        unsigned int dstBit = 0;
        while(size > 0) {
            /* copy bit */
            if (rawValue & (1ULL<<dstBit)) {
                msgData[srcBit/8] |= (1<<(srcBit%8));
            } else {
                msgData[srcBit/8] &= ~(1<<(srcBit%8));
            }

            /* calculate next position */
            ++srcBit;
            ++dstBit;
            --size;
        }
    }
//    m_rawValue = static_cast<double>(rawValue);
//    m_phyValue = rawToPhysicalValue(m_rawValue);
}

QStringList FBSignal::getValueDescriptionTextAll() const
{
    QStringList sl;

    if (!m_valueDescriptions.isEmpty()) {
        for (auto ci = m_valueDescriptions.constBegin();
                ci != m_valueDescriptions.constEnd();
                ci++) {
            sl.append(ci.value());
        }
    }
    return sl;
}

QString FBSignal::getValueDescriptionText(double rawValue) const
{
    //quint64 v = static_cast<quint64>(rawValue);
    QString outputString;
    if (m_valueDescriptions.count() > 0) { //if this is a value list type then look it up and display the proper string
        auto it = m_valueDescriptions.find(rawValue);
        if ((it != m_valueDescriptions.end())
                /*&& (static_cast<uint64_t>(it.key()) == v)*/) {
            outputString = it.value();
        }
    }

    return outputString;
}

bool FBSignal::getValueDescriptionValue(const QString &value, double &v) const
{
    if (!m_valueDescriptions.isEmpty()) {
        for (auto ci = m_valueDescriptions.constBegin();
                ci != m_valueDescriptions.constEnd();
                ci++) {
            if (ci.value() == value) {
                v = ci.key();
                return true;;
            }
        }
    }

    return false;
}

void FBSignal::encodePhy(uint8_t *msgData, double phyValue) const
{
    double rowDoubleValue = physicalToRawValue(phyValue);
    quint64 rawValue = static_cast<quint64>(llround(rowDoubleValue));
    encode(msgData, rawValue);
}

double FBSignal::decodePhy(const uint8_t *msgData) const
{
    uint64_t rawValue = decode(msgData);
    double phyValue = rawToPhysicalValue(rawValue);

    return phyValue;
}

quint16 FBSignal::getStartBitByPosition(quint16 bitPosition,
                                        bool isHighLowByteOrder,
                                        quint16 bitSize)
{
    quint16 startBit = 0;

    if (isHighLowByteOrder) {
        // bitPostion is the highest bit(at lower byte) of the signal
        quint16 lowerByteBits = (bitPosition % 8 + 1);
        bool isMultiBytes = (lowerByteBits < bitSize) ? true : false;
        if (!isMultiBytes) {
            startBit = bitPosition - bitSize + 1;
        } else {
            // get next byte highest bit position
            quint16 netxByteMsbPostion = ((bitPosition+1)/8 + 2)*8 - 1;
            quint16 leftBits = static_cast<quint16>(bitSize) - lowerByteBits;
            while (leftBits > 8) {
                netxByteMsbPostion += 8;
                leftBits -= 8;
            }

            startBit = netxByteMsbPostion - leftBits + 1;
        }
    } else {
        startBit = bitPosition;
    }

    //qDebug() << "m_startBit = " << m_startBit;

    return startBit;
}

bool FBSignal::setStartBitByPosition(quint16 bitPosition)
{
    m_startBit = getStartBitByPosition(bitPosition, m_isHighLowByteOrder, m_bitSize);
    return true;
}

} // FIBEX
} // ASAM
